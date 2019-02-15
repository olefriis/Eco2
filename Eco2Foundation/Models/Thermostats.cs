using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Eco2.Models
{
    public class Thermostats
    {
        public static string xmlPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/.eco2.xml";
        public List<Thermostat> thermostats = new List<Thermostat>();
        static readonly XmlSerializer serializer = new XmlSerializer(typeof(Thermostat[]), new XmlRootAttribute("Thermostats"));

        public static Thermostats Read()
        {
            var result = new Thermostats();
            if (File.Exists(xmlPath))
            {
                using (var stream = new FileStream(xmlPath, FileMode.Open))
                {
                    XmlReader reader = new XmlTextReader(stream);
                    var thermostats = (Thermostat[])serializer.Deserialize(reader);
                    result.thermostats.AddRange(thermostats);
                }
            }
            return result;
        }

        public void Write()
        {
            TextWriter writer = new StreamWriter(xmlPath);
            serializer.Serialize(writer, thermostats.ToArray());
        }

        public void RemoveThermostatWithSerial(string serial)
        {
            thermostats.RemoveAll(t => t.Serial == serial);
        }

        public bool HasSecretFor(string serial)
        {
            var thermostat = thermostats.Find(t => t.Serial == serial);
            return thermostat != null && thermostat.SecretKey != null;
        }

        public Thermostat ThermostatWithSerial(string serial)
        {
            var thermostat = thermostats.Find(t => t.Serial == serial);
            if (thermostat == null)
            {
                thermostat = new Thermostat();
                thermostat.Serial = serial;
                thermostats.Add(thermostat);
            }
            return thermostat;
        }
    }
}
