using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeebookConnector
{
    public class AulaModels
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public static ProfileContext? ConvertProfileContext(string jsonString)
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<ProfileContext>(jsonString, _jsonSerializerOptions);
            return result;
        }

        public static AulaPlanResponse? ConvertAulaPlanResponse(string jsonString)
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<AulaPlanResponse>(jsonString, _jsonSerializerOptions);
            return result;
        }
    }

    public class ProfileContext
    {
        public Status Status { get; set; }
        public AulaData Data { get; set; }
        public int Version { get; set; }
        public string Module { get; set; }
        public string Method { get; set; }
    }

    public class Status
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }

    public class AulaData
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PortalRole { get; set; }
        public bool SupportRole { get; set; }
        public bool MunicipalAdmin { get; set; }
        public bool IsGroupHomeAdmin { get; set; }

        public InstitutionProfile InstitutionProfile { get; set; }
        public List<Institution> Institutions { get; set; }
        public List<MunicipalGroup> MunicipalGroups { get; set; }

        public string MobilePhonenumber { get; set; }
        public string HomePhonenumber { get; set; }
        public string WorkPhonenumber { get; set; }

        public PageConfiguration PageConfiguration { get; set; }

        public bool IsSteppedUp { get; set; }
        public string LoginPortalRole { get; set; }
        public List<object> GroupHomes { get; set; }
    }

    public class InstitutionProfile
    {
        public string EncryptionKey { get; set; }
        public bool CommunicationBlock { get; set; }
        public Address Address { get; set; }
        public string Email { get; set; }
        public DateTime? Birthday { get; set; }
        public string Phone { get; set; }

        public List<object> DelegatedCalendarProfiles { get; set; }
        public List<Relation> Relations { get; set; }

        public int Id { get; set; }
        public int ProfileId { get; set; }
        public int UniPersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string Role { get; set; }
        public int MailBoxId { get; set; }

        public Institution Institution { get; set; }
        public string Gender { get; set; }
        public ProfilePicture ProfilePicture { get; set; }
        public string MainGroupName { get; set; }
        public string Metadata { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public int PostalCode { get; set; }
        public string PostalDistrict { get; set; }
    }

    public class Relation
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public int UniPersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string Role { get; set; }
        public int MailBoxId { get; set; }

        public Institution Institution { get; set; }
        public string Gender { get; set; }
        public ProfilePicture ProfilePicture { get; set; }

        public string MainGroupName { get; set; }
        public string Metadata { get; set; }
    }

    public class Institution
    {
        public string InstitutionCode { get; set; }
        public string InstitutionName { get; set; }
        public string MunicipalityCode { get; set; }
        public string MunicipalityName { get; set; }
        public string Type { get; set; }
        public AdministrativeAuthority AdministrativeAuthority { get; set; }
    }

    public class AdministrativeAuthority
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> InstitutionCodes { get; set; }
    }

    public class ProfilePicture
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Bucket { get; set; }
        public bool IsImageScalingPending { get; set; }
        public string Url { get; set; }
    }

    public class InstitutionRoot
    {
        public int InstitutionProfileId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string InstitutionCode { get; set; }
        public string InstitutionType { get; set; }
        public string MunicipalityCode { get; set; }
        public List<Child> Children { get; set; }
        public List<Group> Groups { get; set; }
        public string InstitutionRole { get; set; }
        public List<Permission> Permissions { get; set; }
        public int MailboxId { get; set; }
        public AdministrativeAuthority AdministrativeAuthority { get; set; }
        public bool CommunicationBlock { get; set; }
    }

    public class Child
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public ProfilePicture ProfilePicture { get; set; }
        public string ShortName { get; set; }
        public string InstitutionCode { get; set; }
        public bool HasCustodyOrExtendedAccess { get; set; }
    }

    public class Group
    {
        public int Id { get; set; }
        public string UniGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string GroupType { get; set; }
        public string MembershipType { get; set; }
        public string Role { get; set; }
        public bool DashboardEnabled { get; set; }
        public DateTime? EndTime { get; set; }
        public int? MembershipId { get; set; }
    }

    public class Permission
    {
        public int PermissionId { get; set; }
        public bool StepUp { get; set; }
        public List<int> GroupScopes { get; set; }
        public bool InstitutionScope { get; set; }
    }

    public class MunicipalGroup
    {
        public List<string> MembershipInstitutions { get; set; }
        public int Id { get; set; }
        public string UniGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string GroupType { get; set; }
        public string MembershipType { get; set; }
        public string Role { get; set; }
        public bool DashboardEnabled { get; set; }
        public DateTime? EndTime { get; set; }
        public int? MembershipId { get; set; }
    }

    public class PageConfiguration
    {
        public List<ModuleConfiguration> ModuleConfigurations { get; set; }
        public List<WidgetConfiguration> WidgetConfigurations { get; set; }
        public List<object> EditorPluginDetails { get; set; }
    }

    public class ModuleConfiguration
    {
        public string Placement { get; set; }
        public Module Module { get; set; }
        public int Id { get; set; }
        public int Order { get; set; }
        public string InstitutionRole { get; set; }
        public string Scope { get; set; }
        public string CentralDisplayMode { get; set; }
        public string MunicipalityDisplayMode { get; set; }
        public string InstitutionDisplayMode { get; set; }
        public string AggregatedDisplayMode { get; set; }
        public List<GroupRestriction> RestrictedGroups { get; set; }
    }

    public class Module
    {
        public bool CanBePlacedOnGroup { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
    }

    public class GroupRestriction
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string InstitutionCode { get; set; }
        public string InstitutionName { get; set; }
        public bool MainGroup { get; set; }
        public string UniGroupType { get; set; }
        public bool IsDeactivated { get; set; }
        public bool AllowMembersToBeShown { get; set; }
    }

    public class WidgetConfiguration
    {
        public string Placement { get; set; }
        public Widget Widget { get; set; }
        public int Id { get; set; }
        public int Order { get; set; }
        public string InstitutionRole { get; set; }
        public string Scope { get; set; }
        public string CentralDisplayMode { get; set; }
        public string MunicipalityDisplayMode { get; set; }
        public string InstitutionDisplayMode { get; set; }
        public string AggregatedDisplayMode { get; set; }
        public List<object> RestrictedGroups { get; set; }
    }

    public class Widget
    {
        public string WidgetId { get; set; }
        public bool IsSecure { get; set; }
        public bool CanAccessOnMobile { get; set; }
        public bool CanBePlacedInsideModule { get; set; }
        public bool CanBePlacedOnGroup { get; set; }
        public bool CanBePlacedOnFullPage { get; set; }
        public bool SupportsTestMode { get; set; }
        public string WidgetSupplier { get; set; }
        public bool IsPilot { get; set; }
        public string WidgetVersion { get; set; }
        public string IconEmployee { get; set; }
        public string IconHover { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
    }

    public class AulaPlanResponse
    {
        public List<AulaPlanItem> Items { get; set; } = new();
    }

    public class AulaPlanItem
    {
        public string Id { get; set; }
        public int? SectionId { get; set; }
        public string? Link { get; set; }
        public bool IsVisible { get; set; }
        public List<string> Categories { get; set; }
        public int StudentId { get; set; }
        public int AnnualPlanId { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }

        public PlanItemType ItemType => Enum.TryParse<PlanItemType>(Type, ignoreCase: true, out var itemType) ? itemType : PlanItemType.Comment;
        public string Subject => string.Join(", ", Categories ?? ["Ukendt fag"]);
    }

    public class TokenModel
    {
        public string? JwtToken { get; set; }
        public string? CsrfpToken { get; set; }
    }

    public class Student
    {
        [BsonId]
        public int RelationId { get; set; }
        public int StudentId { get; set; }
        public string? UniloginName { get; set; }
        public string? ShortName { get; set; }
        public string? FullName { get; set; }

        public Student()
        {
            
        }

        public Student(int relationId)
        {
            RelationId = relationId;
        }
    }

    public enum PlanItemType
    {
        Comment = 0,
        Task = 1,
        Assignment = 2,
    }

    public class CalenderRoot
    {
        public List<CalenderEvent>? Data { get; set; }
        public Status Status { get; set; }
    }

    public class CalenderEvent
    {
        public string Title { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Type { get; set; }
        public List<int>? BelongsToProfiles { get; set; }

    }

    public class  MailAddressModel()
    {
        public int Id { get; set; }
        public string Address { get; set; }
    }
}
