namespace AkyildizYonetim.Application.Common.Email;

public static class EmailTemplates
{
    public static string Invitation(string appName, string link)
    {
        return $@"
        <div style='font-family:Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
            <h2 style='color: #4f46e5;'>{appName} Davet</h2>
            <p style='font-size: 16px; color: #4b5563;'>Hesabınızı oluşturmak için aşağıdaki bağlantıya tıklayarak şifrenizi belirleyin:</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{link}' style='padding: 12px 24px; background-color: #4f46e5; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>Hesap Oluştur</a>
            </div>
            <p style='font-size: 14px; color: #9ca3af;'>Bu bağlantı 3 gün boyunca geçerlidir. Eğer bu isteği siz yapmadıysanız lütfen bu e-postayı dikkate almayın.</p>
        </div>";
    }

    public static string PasswordReset(string appName, string link)
    {
        return $@"
        <div style='font-family:Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
            <h2 style='color: #f59e0b;'>Şifre Sıfırlama</h2>
            <p style='font-size: 16px; color: #4b5563;'>Şifrenizi sıfırlamak için lütfen aşağıdaki bağlantıya tıklayın:</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{link}' style='padding: 12px 24px; background-color: #f59e0b; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>Şifremi Sıfırla</a>
            </div>
            <p style='font-size: 14px; color: #9ca3af;'>Bu bağlantı 24 saat boyunca geçerlidir. Eğer bu talebi siz yapmadıysanız lütfen bu e-postayı dikkate almayın.</p>
        </div>";
    }
}
