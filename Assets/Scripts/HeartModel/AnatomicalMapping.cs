using System.Collections.Generic;
using UnityEngine;

namespace HeartLabVR.HeartModel
{
    /// <summary>
    /// Maps anatomical structures to their properties and relationships
    /// Provides detailed information about heart parts and their functions
    /// </summary>
    public class AnatomicalMapping : MonoBehaviour
    {
        [Header("Mapping Configuration")]
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool enableDetailedMapping = true;
        
        private Dictionary<string, AnatomicalStructure> anatomicalMap;
        private Dictionary<HeartPartType, List<string>> typeMapping;
        private Dictionary<string, List<string>> connectionMap;

        public int TotalStructures => anatomicalMap?.Count ?? 0;

        [System.Serializable]
        public class AnatomicalStructure
        {
            public string name;
            public string displayName;
            public string turkishName;
            public HeartPartType type;
            public string description;
            public string function;
            public Vector3 position;
            public List<string> connectedParts;
            public Dictionary<string, string> properties;

            public AnatomicalStructure()
            {
                connectedParts = new List<string>();
                properties = new Dictionary<string, string>();
            }
        }

        private void Start()
        {
            if (loadOnStart)
            {
                InitializeAnatomicalMapping();
            }
        }

        private void InitializeAnatomicalMapping()
        {
            anatomicalMap = new Dictionary<string, AnatomicalStructure>();
            typeMapping = new Dictionary<HeartPartType, List<string>>();
            connectionMap = new Dictionary<string, List<string>>();

            CreateHeartChambers();
            CreateHeartValves();
            CreateBloodVessels();
            CreateHeartMuscle();
            CreateConnections();

            Debug.Log($"Anatomical mapping initialized with {anatomicalMap.Count} structures");
        }

        private void CreateHeartChambers()
        {
            // Left Ventricle - Sol Ventrikül
            var leftVentricle = new AnatomicalStructure
            {
                name = "sol_ventrikul",
                displayName = "Left Ventricle",
                turkishName = "Sol Ventrikül",
                type = HeartPartType.Chamber,
                description = "The left ventricle is the largest and strongest chamber of the heart.",
                function = "Pumps oxygenated blood to the body through the aorta.",
                position = new Vector3(-0.5f, -0.2f, 0f)
            };
            leftVentricle.properties["wall_thickness"] = "thick";
            leftVentricle.properties["pressure"] = "high";
            leftVentricle.properties["blood_type"] = "oxygenated";
            anatomicalMap["sol_ventrikul"] = leftVentricle;

            // Right Ventricle - Sağ Ventrikül
            var rightVentricle = new AnatomicalStructure
            {
                name = "sag_ventrikul",
                displayName = "Right Ventricle",
                turkishName = "Sağ Ventrikül",
                type = HeartPartType.Chamber,
                description = "The right ventricle pumps blood to the lungs.",
                function = "Pumps deoxygenated blood to the lungs through the pulmonary artery.",
                position = new Vector3(0.5f, -0.2f, 0f)
            };
            rightVentricle.properties["wall_thickness"] = "medium";
            rightVentricle.properties["pressure"] = "medium";
            rightVentricle.properties["blood_type"] = "deoxygenated";
            anatomicalMap["sag_ventrikul"] = rightVentricle;

            // Left Atrium - Sol Atrium
            var leftAtrium = new AnatomicalStructure
            {
                name = "sol_atrium",
                displayName = "Left Atrium",
                turkishName = "Sol Atrium",
                type = HeartPartType.Chamber,
                description = "The left atrium receives oxygenated blood from the lungs.",
                function = "Receives oxygenated blood from pulmonary veins and passes it to the left ventricle.",
                position = new Vector3(-0.3f, 0.3f, 0f)
            };
            leftAtrium.properties["wall_thickness"] = "thin";
            leftAtrium.properties["pressure"] = "low";
            leftAtrium.properties["blood_type"] = "oxygenated";
            anatomicalMap["sol_atrium"] = leftAtrium;

            // Right Atrium - Sağ Atrium
            var rightAtrium = new AnatomicalStructure
            {
                name = "sag_atrium",
                displayName = "Right Atrium",
                turkishName = "Sağ Atrium",
                type = HeartPartType.Chamber,
                description = "The right atrium receives deoxygenated blood from the body.",
                function = "Receives deoxygenated blood from vena cava and passes it to the right ventricle.",
                position = new Vector3(0.3f, 0.3f, 0f)
            };
            rightAtrium.properties["wall_thickness"] = "thin";
            rightAtrium.properties["pressure"] = "low";
            rightAtrium.properties["blood_type"] = "deoxygenated";
            anatomicalMap["sag_atrium"] = rightAtrium;

            // Add to type mapping
            AddToTypeMapping(HeartPartType.Chamber, "sol_ventrikul", "sag_ventrikul", "sol_atrium", "sag_atrium");
        }

        private void CreateHeartValves()
        {
            // Mitral Valve - Mitral Kapak
            var mitralValve = new AnatomicalStructure
            {
                name = "mitral_kapak",
                displayName = "Mitral Valve",
                turkishName = "Mitral Kapak",
                type = HeartPartType.Valve,
                description = "The mitral valve controls blood flow between the left atrium and left ventricle.",
                function = "Prevents backflow of blood from left ventricle to left atrium.",
                position = new Vector3(-0.4f, 0f, 0f)
            };
            mitralValve.properties["leaflets"] = "2";
            mitralValve.properties["location"] = "atrioventricular";
            anatomicalMap["mitral_kapak"] = mitralValve;

            // Tricuspid Valve - Triküspit Kapak
            var tricuspidValve = new AnatomicalStructure
            {
                name = "trikuspid_kapak",
                displayName = "Tricuspid Valve",
                turkishName = "Triküspit Kapak",
                type = HeartPartType.Valve,
                description = "The tricuspid valve controls blood flow between the right atrium and right ventricle.",
                function = "Prevents backflow of blood from right ventricle to right atrium.",
                position = new Vector3(0.4f, 0f, 0f)
            };
            tricuspidValve.properties["leaflets"] = "3";
            tricuspidValve.properties["location"] = "atrioventricular";
            anatomicalMap["trikuspid_kapak"] = tricuspidValve;

            // Aortic Valve - Aort Kapağı
            var aorticValve = new AnatomicalStructure
            {
                name = "aort_kapagi",
                displayName = "Aortic Valve",
                turkishName = "Aort Kapağı",
                type = HeartPartType.Valve,
                description = "The aortic valve controls blood flow from the left ventricle to the aorta.",
                function = "Prevents backflow of blood from aorta to left ventricle.",
                position = new Vector3(-0.2f, -0.4f, 0f)
            };
            aorticValve.properties["leaflets"] = "3";
            aorticValve.properties["location"] = "semilunar";
            anatomicalMap["aort_kapagi"] = aorticValve;

            // Pulmonary Valve - Pulmoner Kapak
            var pulmonaryValve = new AnatomicalStructure
            {
                name = "pulmoner_kapak",
                displayName = "Pulmonary Valve",
                turkishName = "Pulmoner Kapak",
                type = HeartPartType.Valve,
                description = "The pulmonary valve controls blood flow from the right ventricle to the pulmonary artery.",
                function = "Prevents backflow of blood from pulmonary artery to right ventricle.",
                position = new Vector3(0.2f, -0.4f, 0f)
            };
            pulmonaryValve.properties["leaflets"] = "3";
            pulmonaryValve.properties["location"] = "semilunar";
            anatomicalMap["pulmoner_kapak"] = pulmonaryValve;

            AddToTypeMapping(HeartPartType.Valve, "mitral_kapak", "trikuspid_kapak", "aort_kapagi", "pulmoner_kapak");
        }

        private void CreateBloodVessels()
        {
            // Aorta
            var aorta = new AnatomicalStructure
            {
                name = "aorta",
                displayName = "Aorta",
                turkishName = "Aorta",
                type = HeartPartType.Vessel,
                description = "The aorta is the largest artery in the body.",
                function = "Carries oxygenated blood from the left ventricle to the body.",
                position = new Vector3(-0.2f, -0.6f, 0f)
            };
            aorta.properties["vessel_type"] = "artery";
            aorta.properties["blood_type"] = "oxygenated";
            aorta.properties["diameter"] = "large";
            anatomicalMap["aorta"] = aorta;

            // Pulmonary Artery - Pulmoner Arter
            var pulmonaryArtery = new AnatomicalStructure
            {
                name = "pulmoner_arter",
                displayName = "Pulmonary Artery",
                turkishName = "Pulmoner Arter",
                type = HeartPartType.Vessel,
                description = "The pulmonary artery carries blood to the lungs.",
                function = "Carries deoxygenated blood from the right ventricle to the lungs.",
                position = new Vector3(0.2f, -0.6f, 0f)
            };
            pulmonaryArtery.properties["vessel_type"] = "artery";
            pulmonaryArtery.properties["blood_type"] = "deoxygenated";
            pulmonaryArtery.properties["diameter"] = "medium";
            anatomicalMap["pulmoner_arter"] = pulmonaryArtery;

            // Vena Cava - Vena Kava
            var venaCava = new AnatomicalStructure
            {
                name = "vena_kava",
                displayName = "Vena Cava",
                turkishName = "Vena Kava",
                type = HeartPartType.Vessel,
                description = "The vena cava returns blood to the heart from the body.",
                function = "Returns deoxygenated blood from the body to the right atrium.",
                position = new Vector3(0.3f, 0.6f, 0f)
            };
            venaCava.properties["vessel_type"] = "vein";
            venaCava.properties["blood_type"] = "deoxygenated";
            venaCava.properties["diameter"] = "large";
            anatomicalMap["vena_kava"] = venaCava;

            // Pulmonary Veins - Pulmoner Venler
            var pulmonaryVeins = new AnatomicalStructure
            {
                name = "pulmoner_venler",
                displayName = "Pulmonary Veins",
                turkishName = "Pulmoner Venler",
                type = HeartPartType.Vessel,
                description = "The pulmonary veins return oxygenated blood from the lungs.",
                function = "Returns oxygenated blood from the lungs to the left atrium.",
                position = new Vector3(-0.3f, 0.6f, 0f)
            };
            pulmonaryVeins.properties["vessel_type"] = "vein";
            pulmonaryVeins.properties["blood_type"] = "oxygenated";
            pulmonaryVeins.properties["diameter"] = "medium";
            anatomicalMap["pulmoner_venler"] = pulmonaryVeins;

            AddToTypeMapping(HeartPartType.Vessel, "aorta", "pulmoner_arter", "vena_kava", "pulmoner_venler");
        }

        private void CreateHeartMuscle()
        {
            // Myocardium - Kalp Kası
            var myocardium = new AnatomicalStructure
            {
                name = "miyokard",
                displayName = "Myocardium",
                turkishName = "Miyokard (Kalp Kası)",
                type = HeartPartType.Muscle,
                description = "The myocardium is the muscular layer of the heart wall.",
                function = "Contracts to pump blood through the circulatory system.",
                position = new Vector3(0f, 0f, 0f)
            };
            myocardium.properties["muscle_type"] = "cardiac";
            myocardium.properties["contraction"] = "involuntary";
            anatomicalMap["miyokard"] = myocardium;

            AddToTypeMapping(HeartPartType.Muscle, "miyokard");
        }

        private void CreateConnections()
        {
            // Define anatomical connections
            AddConnection("sol_atrium", "mitral_kapak", "sol_ventrikul");
            AddConnection("sag_atrium", "trikuspid_kapak", "sag_ventrikul");
            AddConnection("sol_ventrikul", "aort_kapagi", "aorta");
            AddConnection("sag_ventrikul", "pulmoner_kapak", "pulmoner_arter");
            AddConnection("vena_kava", "sag_atrium");
            AddConnection("pulmoner_venler", "sol_atrium");
        }

        private void AddToTypeMapping(HeartPartType type, params string[] partNames)
        {
            if (!typeMapping.ContainsKey(type))
            {
                typeMapping[type] = new List<string>();
            }

            foreach (string partName in partNames)
            {
                typeMapping[type].Add(partName);
            }
        }

        private void AddConnection(string from, params string[] toList)
        {
            if (!connectionMap.ContainsKey(from))
            {
                connectionMap[from] = new List<string>();
            }

            foreach (string to in toList)
            {
                connectionMap[from].Add(to);
                
                // Also update the anatomical structure
                if (anatomicalMap.ContainsKey(from))
                {
                    anatomicalMap[from].connectedParts.Add(to);
                }
            }
        }

        /// <summary>
        /// Gets anatomical structure information by name
        /// </summary>
        /// <param name="structureName">Name of the structure</param>
        /// <returns>Anatomical structure or null if not found</returns>
        public AnatomicalStructure GetStructure(string structureName)
        {
            return anatomicalMap.ContainsKey(structureName) ? anatomicalMap[structureName] : null;
        }

        /// <summary>
        /// Gets all structures of a specific type
        /// </summary>
        /// <param name="type">Heart part type</param>
        /// <returns>List of structure names</returns>
        public List<string> GetStructuresByType(HeartPartType type)
        {
            return typeMapping.ContainsKey(type) ? new List<string>(typeMapping[type]) : new List<string>();
        }

        /// <summary>
        /// Gets structures connected to a specific part
        /// </summary>
        /// <param name="structureName">Name of the structure</param>
        /// <returns>List of connected structure names</returns>
        public List<string> GetConnectedStructures(string structureName)
        {
            return connectionMap.ContainsKey(structureName) ? new List<string>(connectionMap[structureName]) : new List<string>();
        }

        /// <summary>
        /// Gets detailed description for a structure
        /// </summary>
        /// <param name="structureName">Name of the structure</param>
        /// <param name="language">Language preference ("tr" for Turkish, "en" for English)</param>
        /// <returns>Detailed description</returns>
        public string GetDetailedDescription(string structureName, string language = "tr")
        {
            var structure = GetStructure(structureName);
            if (structure == null)
                return "";

            if (language == "tr")
            {
                return $"{structure.turkishName}: {structure.description} Görevi: {structure.function}";
            }
            else
            {
                return $"{structure.displayName}: {structure.description} Function: {structure.function}";
            }
        }

        /// <summary>
        /// Gets blood flow path from one structure to another
        /// </summary>
        /// <param name="from">Starting structure</param>
        /// <param name="to">Destination structure</param>
        /// <returns>Blood flow path as list of structure names</returns>
        public List<string> GetBloodFlowPath(string from, string to)
        {
            var path = new List<string>();
            var visited = new HashSet<string>();
            
            if (FindPath(from, to, path, visited))
            {
                return path;
            }
            
            return new List<string>();
        }

        private bool FindPath(string current, string target, List<string> path, HashSet<string> visited)
        {
            if (visited.Contains(current))
                return false;

            visited.Add(current);
            path.Add(current);

            if (current == target)
                return true;

            if (connectionMap.ContainsKey(current))
            {
                foreach (string next in connectionMap[current])
                {
                    if (FindPath(next, target, path, visited))
                        return true;
                }
            }

            path.RemoveAt(path.Count - 1);
            return false;
        }

        /// <summary>
        /// Gets all available structure names
        /// </summary>
        /// <returns>List of all structure names</returns>
        public List<string> GetAllStructureNames()
        {
            return new List<string>(anatomicalMap.Keys);
        }

        /// <summary>
        /// Validates if a structure name exists
        /// </summary>
        /// <param name="structureName">Name to validate</param>
        /// <returns>True if structure exists</returns>
        public bool StructureExists(string structureName)
        {
            return anatomicalMap.ContainsKey(structureName);
        }

        /// <summary>
        /// Gets anatomical mapping statistics
        /// </summary>
        /// <returns>Statistics as formatted string</returns>
        public string GetMappingStats()
        {
            int chambers = GetStructuresByType(HeartPartType.Chamber).Count;
            int valves = GetStructuresByType(HeartPartType.Valve).Count;
            int vessels = GetStructuresByType(HeartPartType.Vessel).Count;
            int muscles = GetStructuresByType(HeartPartType.Muscle).Count;
            
            return $"Total Structures: {TotalStructures}, Chambers: {chambers}, Valves: {valves}, " +
                   $"Vessels: {vessels}, Muscles: {muscles}, Connections: {connectionMap.Count}";
        }
    }
}