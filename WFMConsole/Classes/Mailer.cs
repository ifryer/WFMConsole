using Mvc.Mailer;
using System.Net.Mail;
namespace WFMConsole.Classes
{
    public class Mailer : MailerBase
    {
        public MvcMailMessage ReportEmail(string reportContent, string msgTo, string msgFrom, string subject)
        {
            //ViewBag.Model = data;

            return Populate(m =>
            {
                m.Body = reportContent;
                //m.ViewName = "MailView";
                m.To.Add(msgTo);
                m.Subject = subject;
                m.From = new MailAddress(msgFrom);
            });

        }
    }
}