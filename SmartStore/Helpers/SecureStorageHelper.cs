using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SmartStorePOS.Helpers
{
    public class AppConfig
    {
        public string? CameraIp { get; set; }
        public string? DeviceId { get; set; }
    }

    public static class SecureStorageHelper
    {
        private static readonly string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "auth_token.dat");
        private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "config.dat");

        // Mã hóa token trước khi lưu
        public static void SaveToken(string token)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(token);
                byte[] encryptedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(FilePath, encryptedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu token: {ex.Message}");
            }
        }

        // Giải mã token khi cần sử dụng
        public static string GetToken()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return string.Empty;

                byte[] encryptedData = File.ReadAllBytes(FilePath);
                byte[] decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decryptedData);
            }
            catch
            {
                return string.Empty;
            }
        }

        // Xóa token khi đăng xuất
        public static void ClearToken()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }

        // Lưu cấu hình
        public static void SaveConfig(AppConfig config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config);
                byte[] data = Encoding.UTF8.GetBytes(json);
                byte[] encryptedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(ConfigPath, encryptedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu cấu hình: {ex.Message}");
            }
        }

        // Đọc cấu hình
        public static AppConfig? GetConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return null;

                byte[] encryptedData = File.ReadAllBytes(ConfigPath);
                byte[] decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
                string json = Encoding.UTF8.GetString(decryptedData);
                return JsonConvert.DeserializeObject<AppConfig>(json);
            }
            catch
            {
                return new AppConfig();
            }
        }

        // Lưu cấu hình theo key
        public static void SaveConfigDynamic(string key, string value)
        {
            try
            {
                AppConfig config = GetConfig();
                config ??= new AppConfig();

                if (config.GetType().GetProperty(key) != null)
                {
                    config.GetType().GetProperty(key).SetValue(config, value);
                }
                else
                {
                    // nếu chưa có thì thêm mới
                    config.GetType().GetProperty(key).SetValue(config, value);
                }
                SaveConfig(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu cấu hình: {ex.Message}");
            }
        }

        // Đọc cấu hình theo key
        public static string GetConfigDynamic(string key)
        {
            try
            {
                AppConfig config = GetConfig();
                if (config == null) return string.Empty;
                if (config.GetType().GetProperty(key) != null)
                {
                    return config.GetType().GetProperty(key).GetValue(config).ToString();
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
