using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace QTechClassroom
{
    public partial class Main : Window
    {
        readonly string TempPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"qtc9aff89.tmp";

        void SaveTemp()
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, URP.CookieContainer);
                bf.Serialize(ms, txtUser.Text);
                bf.Serialize(ms, txtPass.Password);
                var bytes = Encoding.ASCII.GetBytes(@"qtc9aff89.tmp");
                bytes = ProtectedData.Protect(ms.ToArray(), bytes, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(TempPath, bytes);
            }
        }

        void LoadTemp()
        {
            if (!File.Exists(TempPath)) return;
            try
            {
                var bytes = File.ReadAllBytes(TempPath);
                var mix = Encoding.ASCII.GetBytes(@"qtc9aff89.tmp");
                bytes = ProtectedData.Unprotect(bytes, mix, DataProtectionScope.CurrentUser);
                using (var ms = new MemoryStream(bytes))
                {
                    var bf = new BinaryFormatter();
                    URP.CookieContainer = bf.Deserialize(ms) as System.Net.CookieContainer;
                    txtUser.Text = bf.Deserialize(ms).ToString();
                    txtPass.Password = bf.Deserialize(ms).ToString();
                }
            }
            catch { }
        }
    }
}
