
namespace check_headers;

public class NntpCodes
{
    public static Dictionary<int, string> NntpResponseCodes = new Dictionary<int, string>
    {
        { 100, "Continue" },
        { 199, "Debug Output" },
        { 200, "Server Ready - Posting Allowed" },
        { 201, "Server Ready - Posting Not Allowed" },
        { 202, "Slave Status Noted" },
        { 205, "Closing Connection" },
        { 211, "Group Selected" },
        { 220, "Article Retrieved - Head and Body Follow" },
        { 221, "Article Retrieved - Head Follows" },
        { 222, "Article Retrieved - Body Follows" },
        { 223, "Article Retrieved - Request Text Separately" },
        { 230, "Article List by Message-ID Follows" },
        { 231, "New Newsgroup List Follows" },
        { 235, "Article Transferred OK" },
        { 240, "Article Posted OK" },
        { 281, "Authentication Accepted" },
        { 335, "Send Article to Transfer" },
        { 340, "Send Article to Post" },
        { 381, "More Authentication Info Required" },
        { 400, "Service Discontinued" },
        { 411, "No Such Newsgroup" },
        { 408, "Authentication Required" },
        { 412, "No Newsgroup Selected" },
        { 420, "No Current Article Selected" },
        { 421, "No Next Article" },
        { 422, "No Previous Article" },
        { 423, "No Such Article Number" },
        { 430, "No Such Article Found" },
        { 435, "Article Not Wanted" },
        { 436, "Transfer Failed" },
        { 437, "Article Rejected" },
        { 440, "Posting Not Allowed" },
        { 441, "Posting Failed" },
        { 482, "Authentication Rejected" },
        { 500, "Command Not Recognized" },
        { 501, "Command Syntax Error" },
        { 502, "Permission Denied" },
        { 503, "Program Fault" }
    };

    public static string GetResponseMessage(int code)
    {
        if (NntpResponseCodes.TryGetValue(code, out string? message))
        {
            return message;
        }
        else
        {
            return "Unknown Response Code";
        }
    }
}
