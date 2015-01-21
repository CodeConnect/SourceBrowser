using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System;

namespace SourceBrowser.Site.Utilities
{
    public static class FileUtilities
    {
        /// <summary>
        /// Opens the file specified,
        /// deserializes its content assuming binary serialization
        /// and casts the resulting object to a specified type.
        /// </summary>
        /// <typeparam name="T">Type of the serialized object</typeparam>
        /// <param name="filePath">Path that points to serialized data</param>
        /// <returns>Deserialized object</returns>
        public static T DeserializeData<T>(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (T)formatter.Deserialize(fs);
                }
                catch (SerializationException e)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Serializes specified object into a file, using binary serialization
        /// </summary>
        /// <param name="data">Object to be serialized</param>
        /// <param name="filePath">Path to store serialized data</param>
        public static void SerializeData(object data, string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, data);
                }
                catch (SerializationException e)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns true if the file specified exists and was modified within last 120 minutes.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>true if the file specified exists and was modified within last 120 minutes.</returns>
        public static bool FileIsFresh(string filePath)
        {
            // If file doesn't exist, it's not "fresh" (it needs to be downloaded)
            if (!File.Exists(filePath))
            {
                return false;
            }

            // If file has been modified within last 60 minutes, it is considered "fresh"
            DateTime freshThreshold = DateTime.Now - TimeSpan.FromDays(7);
            var lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            var thresholdTime = freshThreshold.ToUniversalTime();
            if (lastWriteTime > thresholdTime)
            {
                return true;
            }

            return false;
        }

    }
}