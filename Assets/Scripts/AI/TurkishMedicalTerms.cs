using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HeartLabVR.Utilities;

namespace HeartLabVR.AI
{
    /// <summary>
    /// Manages Turkish medical terminology for heart anatomy education
    /// Provides translation and context for medical terms
    /// </summary>
    public class TurkishMedicalTerms : MonoBehaviour
    {
        [Header("Language Configuration")]
        [SerializeField] private bool enableAutoTranslation = true;
        [SerializeField] private bool logTermUsage = false;

        private Dictionary<string, MedicalTerm> medicalDictionary;
        private Dictionary<string, List<string>> categoryTerms;

        [System.Serializable]
        public class MedicalTerm
        {
            public string turkish;
            public string english;
            public string definition;
            public string category;
            public string pronunciation;
            
            public MedicalTerm(string tr, string en, string def, string cat, string pronun = "")
            {
                turkish = tr;
                english = en;
                definition = def;
                category = cat;
                pronunciation = pronun;
            }
        }

        private void Awake()
        {
            InitializeMedicalDictionary();
        }

        private void InitializeMedicalDictionary()
        {
            medicalDictionary = new Dictionary<string, MedicalTerm>();
            categoryTerms = new Dictionary<string, List<string>>();

            // Heart chambers - Kalp odacıkları
            AddMedicalTerm("sol ventrikül", "left ventricle", "Kalbin sol alt odacığı, vücuda kan pompalar", "odacik", "sol ven-tri-kül");
            AddMedicalTerm("sağ ventrikül", "right ventricle", "Kalbin sağ alt odacığı, akciğerlere kan pompalar", "odacik", "sağ ven-tri-kül");
            AddMedicalTerm("sol atrium", "left atrium", "Kalbin sol üst odacığı, akciğerlerden kan alır", "odacik", "sol at-ri-um");
            AddMedicalTerm("sağ atrium", "right atrium", "Kalbin sağ üst odacığı, vücuttan kan alır", "odacik", "sağ at-ri-um");

            // Heart valves - Kalp kapakları
            AddMedicalTerm("mitral kapak", "mitral valve", "Sol atrium ile sol ventrikül arasındaki kapak", "kapak", "mit-ral ka-pak");
            AddMedicalTerm("triküspit kapak", "tricuspid valve", "Sağ atrium ile sağ ventrikül arasındaki kapak", "kapak", "tri-küs-pit ka-pak");
            AddMedicalTerm("aort kapağı", "aortic valve", "Sol ventrikül ile aorta arasındaki kapak", "kapak", "a-ort ka-pa-ğı");
            AddMedicalTerm("pulmoner kapak", "pulmonary valve", "Sağ ventrikül ile pulmoner arter arasındaki kapak", "kapak", "pul-mo-ner ka-pak");

            // Blood vessels - Kan damarları
            AddMedicalTerm("aorta", "aorta", "Vücudun en büyük arterisi", "damar", "a-or-ta");
            AddMedicalTerm("pulmoner arter", "pulmonary artery", "Kalpteki kanı akciğerlere taşıyan arter", "damar", "pul-mo-ner ar-ter");
            AddMedicalTerm("vena kava", "vena cava", "Vücuttan kalbe kan getiren büyük ven", "damar", "ve-na ka-va");
            AddMedicalTerm("koroner arter", "coronary artery", "Kalp kasını besleyen arterler", "damar", "ko-ro-ner ar-ter");

            // Heart functions - Kalp fonksiyonları
            AddMedicalTerm("sistol", "systole", "Kalbin kasılma dönemi", "fonksiyon", "sis-tol");
            AddMedicalTerm("diyastol", "diastole", "Kalbin gevşeme dönemi", "fonksiyon", "di-yas-tol");
            AddMedicalTerm("kalp atımı", "heartbeat", "Kalbin bir kasılma ve gevşeme döngüsü", "fonksiyon", "kalp a-tı-mı");
            AddMedicalTerm("kalp debisi", "cardiac output", "Kalbin dakikada pompaladığı kan miktarı", "fonksiyon", "kalp de-bi-si");

            // Medical conditions - Tıbbi durumlar
            AddMedicalTerm("aritmi", "arrhythmia", "Düzensiz kalp ritmi", "hastalık", "a-rit-mi");
            AddMedicalTerm("bradikardi", "bradycardia", "Yavaş kalp atışı", "hastalık", "bra-di-kar-di");
            AddMedicalTerm("taşikardi", "tachycardia", "Hızlı kalp atışı", "hastalık", "ta-şi-kar-di");
            AddMedicalTerm("kalp yetmezliği", "heart failure", "Kalbin yetersiz pompalama durumu", "hastalık", "kalp yet-mez-li-ği");

            // Additional terms - Ek terimler
            AddMedicalTerm("kalp kası", "myocardium", "Kalbin kas dokusu", "doku", "kalp ka-sı");
            AddMedicalTerm("perikard", "pericardium", "Kalbi çevreleyen zar", "doku", "pe-ri-kard");
            AddMedicalTerm("endokard", "endocardium", "Kalbin iç yüzeyini döşeyen doku", "doku", "en-do-kard");

            if (logTermUsage)
            {
                Debug.Log($"Initialized medical dictionary with {medicalDictionary.Count} terms");
            }
        }

        private void AddMedicalTerm(string turkish, string english, string definition, string category, string pronunciation = "")
        {
            var term = new MedicalTerm(turkish, english, definition, category, pronunciation);
            medicalDictionary[turkish.ToLowerTurkish()] = term;
            medicalDictionary[english.ToLower()] = term;

            // Add to category grouping
            if (!categoryTerms.ContainsKey(category))
            {
                categoryTerms[category] = new List<string>();
            }
            categoryTerms[category].Add(turkish);
        }

        /// <summary>
        /// Gets relevant medical terms based on user query
        /// </summary>
        /// <param name="query">User's question or statement</param>
        /// <returns>Relevant medical terms as formatted string</returns>
        public string GetRelevantTerms(string query)
        {
            var relevantTerms = new List<MedicalTerm>();
            string lowerQuery = query.ToLowerTurkish();

            foreach (var kvp in medicalDictionary)
            {
                if (lowerQuery.Contains(kvp.Key) || lowerQuery.Contains(kvp.Value.english.ToLower()))
                {
                    if (!relevantTerms.Any(t => t.turkish == kvp.Value.turkish))
                    {
                        relevantTerms.Add(kvp.Value);
                    }
                }
            }

            if (relevantTerms.Count == 0)
                return "";

            return string.Join(", ", relevantTerms.Select(t => $"{t.turkish} ({t.english})"));
        }

        /// <summary>
        /// Translates a medical term between Turkish and English
        /// </summary>
        /// <param name="term">Term to translate</param>
        /// <returns>Translated term or original if not found</returns>
        public string TranslateTerm(string term)
        {
            string lowerTerm = term.ToLowerTurkish();
            
            if (medicalDictionary.ContainsKey(lowerTerm))
            {
                var medicalTerm = medicalDictionary[lowerTerm];
                // If input was Turkish, return English; if English, return Turkish
                return lowerTerm == medicalTerm.turkish.ToLowerTurkish() ? 
                    medicalTerm.english : medicalTerm.turkish;
            }

            return term; // Return original if not found
        }

        /// <summary>
        /// Gets definition of a medical term
        /// </summary>
        /// <param name="term">Medical term</param>
        /// <returns>Definition in Turkish</returns>
        public string GetDefinition(string term)
        {
            string lowerTerm = term.ToLowerTurkish();
            
            if (medicalDictionary.ContainsKey(lowerTerm))
            {
                return medicalDictionary[lowerTerm].definition;
            }

            return "Bu terim sözlükte bulunamadı.";
        }

        /// <summary>
        /// Gets pronunciation guide for a medical term
        /// </summary>
        /// <param name="term">Medical term</param>
        /// <returns>Pronunciation guide</returns>
        public string GetPronunciation(string term)
        {
            string lowerTerm = term.ToLowerTurkish();
            
            if (medicalDictionary.ContainsKey(lowerTerm))
            {
                return medicalDictionary[lowerTerm].pronunciation;
            }

            return "";
        }

        /// <summary>
        /// Gets all terms in a specific category
        /// </summary>
        /// <param name="category">Category name</param>
        /// <returns>List of terms in category</returns>
        public List<string> GetTermsByCategory(string category)
        {
            return categoryTerms.ContainsKey(category) ? 
                new List<string>(categoryTerms[category]) : new List<string>();
        }

        /// <summary>
        /// Gets all available categories
        /// </summary>
        /// <returns>List of category names</returns>
        public List<string> GetCategories()
        {
            return new List<string>(categoryTerms.Keys);
        }

        /// <summary>
        /// Searches for terms containing specific keywords
        /// </summary>
        /// <param name="keyword">Search keyword</param>
        /// <returns>List of matching terms</returns>
        public List<MedicalTerm> SearchTerms(string keyword)
        {
            var results = new List<MedicalTerm>();
            string lowerKeyword = keyword.ToLowerTurkish();

            foreach (var term in medicalDictionary.Values.Distinct())
            {
                if (term.turkish.ToLowerTurkish().Contains(lowerKeyword) ||
                    term.english.ToLower().Contains(lowerKeyword.ToLower()) ||
                    term.definition.ToLowerTurkish().Contains(lowerKeyword))
                {
                    results.Add(term);
                }
            }

            return results.Distinct().ToList();
        }
    }
}