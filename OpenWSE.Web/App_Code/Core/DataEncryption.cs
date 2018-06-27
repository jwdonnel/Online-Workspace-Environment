#region

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

#endregion


namespace OpenWSE.Core {

    /// <summary>
    /// Summary description for DataEncryption
    /// </summary>
    public class DataEncryption {
        public DataEncryption() { }

        internal const string EncryptPassword = "1234512345678976";

        public static void SerializeFile(object _list, string _loc) {
            try {
                using (var str = new BinaryWriter(File.Create(_loc))) {
                    var bf = new BinaryFormatter();
                    bf.Serialize(str.BaseStream, _list);
                    str.Close();
                }
            }
            catch {
            }
        }

        public static void EncryptFile(string inputFile, string outputFile) {
            var aes = new RijndaelManaged();

            try {
                byte[] key = Encoding.UTF8.GetBytes(EncryptPassword);

                using (var fsCrypt = new FileStream(outputFile, FileMode.Create)) {
                    using (var cs = new CryptoStream(fsCrypt, aes.CreateEncryptor(key, key), CryptoStreamMode.Write)) {
                        using (var fsIn = new FileStream(inputFile, FileMode.Open)) {
                            int data;

                            while ((data = fsIn.ReadByte()) != -1) {
                                cs.WriteByte((byte)data);
                            }

                            aes.Clear();
                        }
                    }
                }
                File.Delete(inputFile);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                aes.Clear();
            }
        }

        public static MemoryStream DecryptFile(string inputFile) {
            var aes = new RijndaelManaged();

            try {
                byte[] key = Encoding.UTF8.GetBytes(EncryptPassword);

                using (var fsCrypt = new FileStream(inputFile, FileMode.Open)) {
                    var fsOut = new MemoryStream();

                    using (var cs = new CryptoStream(fsCrypt, aes.CreateDecryptor(key, key), CryptoStreamMode.Read)) {
                        int data;

                        while ((data = cs.ReadByte()) != -1) {
                            fsOut.WriteByte((byte)data);
                        }

                        aes.Clear();
                        return fsOut;
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                aes.Clear();
                return null;
            }
        }
    }

}
