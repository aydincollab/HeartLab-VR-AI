using System.Collections.Generic;
using UnityEngine;
using HeartLabVR.Utilities;

namespace HeartLabVR.AI
{
    /// <summary>
    /// Manages medical prompts and context for AI interactions
    /// Provides Turkish language medical terminology and anatomical context
    /// </summary>
    public class MedicalPromptManager : MonoBehaviour
    {
        [Header("Prompt Configuration")]
        [SerializeField] private bool useTurkishLanguage = true;
        [SerializeField] private string systemPrompt = "Kalp anatomisi konusunda uzman bir tıp eğitmenisiniz. Türkçe olarak açık, anlaşılır ve bilimsel doğru cevaplar verin.";

        private TurkishMedicalTerms medicalTerms;
        private Dictionary<string, string> anatomicalContexts;

        private void Awake()
        {
            medicalTerms = GetComponent<TurkishMedicalTerms>();
            InitializeAnatomicalContexts();
        }

        private void InitializeAnatomicalContexts()
        {
            anatomicalContexts = new Dictionary<string, string>
            {
                ["sol_ventrikul"] = "Sol ventrikül kalbin en güçlü kasılı bölümüdür ve vücuda kan pompalar.",
                ["sag_ventrikul"] = "Sağ ventrikül akciğerlere kan pompalar ve pulmoner dolaşımı sağlar.",
                ["sol_atrium"] = "Sol atrium akciğerlerden oksijen bakımından zengin kanı alır.",
                ["sag_atrium"] = "Sağ atrium vücuttan oksijen bakımından fakir kanı alır.",
                ["mitral_kapak"] = "Mitral kapak sol atrium ile sol ventrikül arasında yer alır.",
                ["triküspit_kapak"] = "Triküspit kapak sağ atrium ile sağ ventrikül arasında yer alır.",
                ["aort_kapak"] = "Aort kapağı sol ventrikülden aorta çıkışını kontrol eder.",
                ["pulmoner_kapak"] = "Pulmoner kapak sağ ventrikülden pulmoner artere çıkışı kontrol eder.",
                ["aorta"] = "Aorta vücudun en büyük arteris ve oksijen bakımından zengin kanı taşır.",
                ["pulmoner_arter"] = "Pulmoner arter oksijen bakımından fakir kanı akciğerlere taşır.",
                ["vena_kava"] = "Vena kava büyük venler olup oksijen bakımından fakir kanı kalbe getirir."
            };
        }

        /// <summary>
        /// Creates a contextualized medical prompt for AI processing
        /// </summary>
        /// <param name="userQuery">User's question in Turkish</param>
        /// <param name="anatomicalPart">Currently selected heart part</param>
        /// <returns>Enhanced prompt with medical context</returns>
        public string CreateMedicalPrompt(string userQuery, string anatomicalPart = "")
        {
            string prompt = systemPrompt + "\n\n";

            // Add anatomical context if available
            if (!string.IsNullOrEmpty(anatomicalPart) && anatomicalContexts.ContainsKey(anatomicalPart))
            {
                prompt += $"Şu anda incelenen kalp bölümü: {anatomicalPart}\n";
                prompt += $"Bu bölüm hakkında: {anatomicalContexts[anatomicalPart]}\n\n";
            }

            // Add medical terminology context
            if (medicalTerms != null)
            {
                string relevantTerms = medicalTerms.GetRelevantTerms(userQuery);
                if (!string.IsNullOrEmpty(relevantTerms))
                {
                    prompt += $"İlgili tıbbi terimler: {relevantTerms}\n\n";
                }
            }

            prompt += $"Kullanıcı sorusu: {userQuery}\n\n";
            prompt += "Lütfen eğitici, anlaşılır ve bilimsel olarak doğru bir cevap verin. Cevabınızı 2-3 cümle ile sınırlayın.";

            return prompt;
        }

        /// <summary>
        /// Provides offline fallback responses for common queries
        /// </summary>
        /// <param name="query">User's query</param>
        /// <param name="anatomicalPart">Selected anatomical part</param>
        /// <returns>Fallback educational content</returns>
        public string GetOfflineFallback(string query, string anatomicalPart = "")
        {
            // Check for common query patterns
            string lowerQuery = query.ToLowerTurkish();

            if (lowerQuery.Contains("ventrikül") || lowerQuery.Contains("ventricle"))
            {
                return "Ventriküller kalbin alt odacıklarıdır. Sol ventrikül vücuda, sağ ventrikül ise akciğerlere kan pompalar.";
            }
            else if (lowerQuery.Contains("atrium") || lowerQuery.Contains("kulakçık"))
            {
                return "Atriumlar kalbin üst odacıklarıdır. Kanı ventriküllere ileten odacıklardır.";
            }
            else if (lowerQuery.Contains("kapak") || lowerQuery.Contains("valve"))
            {
                return "Kalp kapakları kanın tek yönde akmasını sağlar ve geri akışını engeller.";
            }
            else if (!string.IsNullOrEmpty(anatomicalPart) && anatomicalContexts.ContainsKey(anatomicalPart))
            {
                return anatomicalContexts[anatomicalPart];
            }

            return "Kalp, kan dolaşım sistemimizin merkezi olan hayati bir organdır. Dört odacık ve dört kapaktan oluşur.";
        }

        /// <summary>
        /// Gets educational content based on selected anatomical part
        /// </summary>
        /// <param name="anatomicalPart">Selected heart part</param>
        /// <returns>Educational content about the part</returns>
        public string GetAnatomicalEducation(string anatomicalPart)
        {
            if (anatomicalContexts.ContainsKey(anatomicalPart))
            {
                return anatomicalContexts[anatomicalPart];
            }

            return "Bu kalp bölümü hakkında detaylı bilgi almak için sesli soru sorabilirsiniz.";
        }

        /// <summary>
        /// Adds or updates anatomical context information
        /// </summary>
        /// <param name="partName">Anatomical part name</param>
        /// <param name="description">Educational description</param>
        public void AddAnatomicalContext(string partName, string description)
        {
            anatomicalContexts[partName] = description;
        }

        /// <summary>
        /// Gets all available anatomical parts
        /// </summary>
        /// <returns>List of anatomical part names</returns>
        public List<string> GetAvailableAnatomicalParts()
        {
            return new List<string>(anatomicalContexts.Keys);
        }
    }
}