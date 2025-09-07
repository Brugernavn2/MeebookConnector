Et lille projekt, der henter ugeplanen fra Aula/Meebook og sender den pr. mail.
UniLogin gemmes sikkert i Windows Credential Manager, og LiteDB bruges til at gemme data lokalt.

Der er desuden tilføjet lidt AI via llamaSharp, som kan opsummere ugeplanen og fremhæve vigtige punkter såsom lektier.

Jeg har brugt følgende LLM:
https://huggingface.co/unsloth/gemma-3n-E4B-it-GGUF/blob/main/gemma-3n-E4B-it-IQ4_XS.gguf

Stien til modellen er hardcodet i AiService.cs til den downloadede .gguf.
Man kan dog også forbinde til Ollama eller OpenAI, men jeg foretrækker at køre det lokalt.
Projektet er sat op med CUDA-backend, men der findes også andre backends via NuGet.
