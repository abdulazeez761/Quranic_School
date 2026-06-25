using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

// أداة لمرة واحدة: تُنفّذ موافقة OAuth وتخزّن الـ refresh token في نفس مجلد التطبيق.
// بعد تشغيلها بنجاح، يعمل التطبيق والمهمة المجدولة بدون متصفح.

var credentialsPath = args.Length > 0 ? args[0] : @"C:\HafizSecrets\credentials.json";
var tokenFolder = args.Length > 1 ? args[1] : @"C:\HafizSecrets\drive-token";

if (!File.Exists(credentialsPath))
{
    Console.WriteLine("❌ ملف credentials.json غير موجود في المسار المذكور.");
    return 1;
}

UserCredential credential;
await using (var stream = File.OpenRead(credentialsPath))
{
    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        GoogleClientSecrets.FromStream(stream).Secrets,
        new[] { DriveService.Scope.DriveFile },
        "user",
        CancellationToken.None,
        new FileDataStore(tokenFolder, true)
    );
}

// تحقق سريع: نتأكد أن الـ token يعمل فعليًا بجلب معلومات الحساب.
var service = new DriveService(
    new BaseClientService.Initializer
    {
        HttpClientInitializer = credential,
        ApplicationName = "Hafiz backup auth",
    }
);

var aboutRequest = service.About.Get();
aboutRequest.Fields = "user";
var about = await aboutRequest.ExecuteAsync();

Console.WriteLine();
Console.WriteLine($"👤 الحساب: {about.User?.EmailAddress}");
Console.WriteLine("✅ تمت الموافقة بنجاح وتم تخزين الـ token.");
Console.WriteLine($"📁 token محفوظ في: {tokenFolder}");
Console.WriteLine("الآن يمكنك استخدام زر النسخ الاحتياطي والمهمة المجدولة بدون متصفح.");
return 0;
