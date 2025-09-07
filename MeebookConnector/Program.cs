using MeebookConnector;
using System.Globalization;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        try
        {
            int action = SplashScreen.Show();

            MailRepository mailRepo = new();

            switch (action)
            {
                case 0:
                    // Run
                    break;
                case 1:
                    // Clear mail list                    
                    mailRepo.Clear();
                    Console.WriteLine("Mail list cleared. Press Enter to exit.");
                    Console.ReadLine();
                    return;
                case 2:
                    // Clear unilogin
                    Secrets.ClearUnilogin();
                    Console.WriteLine("Unilogin credentials cleared. Press Enter to exit.");
                    Console.ReadLine();
                    return;
                case 3:
                    // Clear mail login
                    Secrets.ClearMailLogin();
                    Console.WriteLine("Mail credentials cleared. Press Enter to exit.");
                    Console.ReadLine();
                    return;
                default:
                    // Should not happen
                    return;
            }
            
            List<string> mailingList = mailRepo.GetAll()?.Select(m => m.Address)?.ToList() ?? new();
            
            if (mailingList.Count == 0)
            {
                string input = "";
                while (string.IsNullOrEmpty(input))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Indtast en mail adresse der skal modtage ugeplanen:");
                    Console.ResetColor();
                    input = Console.ReadLine() ?? "";
                    if (!string.IsNullOrEmpty(input))
                    {
                        // Validate email
                        if (Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        {
                            mailingList.Add(input);
                            mailRepo.Insert(new MailAddressModel() { Address = input });
                            input = "";
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Ugyldig mail adresse. Prøv igen.");
                            Console.ResetColor();
                            input = "";
                        }
                    }

                    if (mailingList.Count > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("Vil du tilføje endnu en mailadresse? (tryk 'y' for ja eller 'n' for nej): ");
                        Console.ResetColor();
                        var answer = Console.ReadLine();
                        if (answer?.ToLower() == "n")
                        {
                            break;
                        }
                    }
                }
            }

            AulaConnector aulaConnector = new AulaConnector();
            CredentialManagement.Credential? unilogin = null;
            bool loggedIn = false;
            while (!loggedIn)
            {
                unilogin = Secrets.GetUnilogin();
                if (unilogin == null)
                {
                    Console.WriteLine("Failed to retrieve unilogin credentials.");
                    return;
                }

                loggedIn = await aulaConnector.LoginAsync(unilogin.Username, unilogin.SecurePassword, cts.Token);

                if (!loggedIn)
                {
                    Console.WriteLine("Login failed");
                    Secrets.ClearUnilogin();
                }
            }

            var mailLogin = Secrets.GetMailLogin();
            if (mailLogin == null)
            {
                Console.WriteLine("Failed to retrieve mail credentials.");
                return;
            }
            
            var tokenModel = await aulaConnector.GetJwtAsync();
            if (tokenModel == null)
            {
                // Failed to get token
                Console.WriteLine("Failed to retrieve token model.");
                return;
            }
            //string? jwt = tokenModel.JwtToken;

            //if (string.IsNullOrEmpty(jwt))
            //{
            //    Console.WriteLine("Failed to retrieve jwt token.");
            //    return;
            //}

            await aulaConnector.GetProfilesByLogin();
            ProfileContext? profileContext = await aulaConnector.GetProfileContext();

            if (profileContext == null)
            {
                // Failed to get profile context
                Console.WriteLine("Failed to retrieve profile context.");
                return;
            }

            List<CalenderEvent>? importantDates = await aulaConnector.GetImportantDates();

            StudentRepository studentRepository = new();
            List<Student> students = studentRepository.GetAll();

            foreach (var relation in profileContext.Data.InstitutionProfile.Relations)
            {
                Student student = students.FirstOrDefault(s => s.RelationId == relation.Id, new Student(relation.Id));
                bool changed = false;
                if (student.FullName != relation.FullName || student.ShortName != relation.ShortName)
                {
                    student.FullName = relation.FullName;
                    student.ShortName = relation.ShortName;
                    changed = true;
                }

                if (string.IsNullOrEmpty(student.UniloginName))
                {
                    // Prompt for unilogin name
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Please enter the unilogin name for {relation.FullName}:");
                    Console.ResetColor();
                    var input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input))
                    {
                        student.UniloginName = input;
                        changed = true;
                    }
                }

                if (changed)
                {
                    studentRepository.InsertOrUpdate(student);
                }
            }

            students = studentRepository.GetAll();

            // Load AI model
            using AiService aiService = new();
            await aiService.LoadModel();

            foreach (var institution in profileContext.Data.Institutions)
            {
                int.TryParse(institution.InstitutionCode, out int institutionCode);
                
                // Just select a student, to load the connection
                string childUniloginUsername = students.FirstOrDefault()?.UniloginName ?? "";

                MeebookApiConnector meebookApi = new(tokenModel);
                await meebookApi.Connect(unilogin!.Username, childUniloginUsername, institutionCode, cts.Token);
                await meebookApi.LoadAuth(unilogin!.Username, childUniloginUsername, institutionCode, cts.Token);
                await meebookApi.LoadStudentIds(students, studentRepository, cts.Token);

                int weekNumber = Helpers.GetWeekNumber(DateTime.UtcNow);
                // If Friday after 16:00, get next week - Edge case at week 52 not accounted for.
                if (DateTime.Now.Day > (int)DayOfWeek.Friday || DateTime.Now.DayOfWeek == DayOfWeek.Friday && DateTime.Now.Hour >= 16)
                {
                    weekNumber += 1;
                }
                
                var weekPlan = await meebookApi.GetWeekPlan(weekNumber: weekNumber, cts.Token);

                if (weekPlan == null || weekPlan.Items.Count == 0)
                {
                    Console.WriteLine($"No week plan items found for {institution.InstitutionName}.");
                    continue;
                }

                foreach (var relation in profileContext.Data.InstitutionProfile.Relations)
                {
                    int studentId = students.FirstOrDefault(s => s.RelationId == relation.Id)?.StudentId ?? 0;

                    if (studentId == 0)
                    {
                        Console.WriteLine($"No student ID found for {relation.FullName}. Skipping...");
                        continue;
                    }

                    var weekPlanForStudent = weekPlan.Items.Where(i => i.StudentId == studentId).ToList();

                    HashSet<string> changedItemIds = new();
                    HashSet<string> noChanges = new();

                    // Sammenlign med tidligere kørsler fra samme uge og find ændringer.
                    AulaPlanRepository repo = new();
                    foreach (var item in weekPlanForStudent)
                    {
                        var existingItem = repo.GetById(item.Id);
                        if (existingItem == null)
                        {
                            repo.Insert(item);
                        }
                        else
                        {
                            if (existingItem.Categories?.FirstOrDefault() != item.Categories?.FirstOrDefault() || existingItem.Text != item.Text)
                            {
                                changedItemIds.Add(existingItem.Id);
                                // Update in db
                                repo.InsertOrUpdate(item);
                            }
                            else
                            {
                                // No changes
                                noChanges.Add(existingItem.Id);
                            }
                        }
                    }

                    if (noChanges.Count == weekPlanForStudent.Count)
                    {
                        Console.WriteLine("No changes found in week plan.");
                    }

                    DateTime startDate = Helpers.FirstDateOfWeek(DateTime.UtcNow.Year, weekNumber);
                    DateTime endDate = startDate.AddDays(4);

                    var importantDatesForStudent = importantDates?.Where(d => (d.BelongsToProfiles?.Any(p => p == relation.Id) ?? false) && d.StartDateTime < startDate.AddDays(14))?.OrderBy(d => d.StartDateTime).ToList() ?? new List<CalenderEvent>();

                    string aiResponse = await aiService.SummarizeWeekPlanItems(weekPlanForStudent);                    

                    string subHeader = $"mandag {startDate.ToString("dd/MM/yyyy")} - fredag {endDate.ToString("dd/MM/yyyy")}";
                    string header = $"Ugeplan for {relation.MainGroupName} - Uge {weekNumber}";
                    string htmlBody = Mail.BuildHtmlBody(weekPlanForStudent, header, subHeader, relation.FullName, changedItemIds, noChanges, aiResponse, importantDatesForStudent);

                    string footer = $"{profileContext.Data.InstitutionProfile.Institution.InstitutionName} - {profileContext.Data.InstitutionProfile.Institution.MunicipalityName}<br/>Data indsamlet af MeebookConnector - Max Celani Mayn Petersen";

                    foreach (var mail in mailingList)
                    {
                        await Mail.SendEmailAsync(mail, $"Ugeplan for {relation.MainGroupName} - Uge {weekNumber}", header, body: htmlBody, footer: footer, from: mailLogin.Username, appPassword: mailLogin.SecurePassword, date: subHeader);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation canceled.");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled error: {ex}");
        }
        finally
        {
            Console.WriteLine("Done. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}