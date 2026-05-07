using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace ServiceDeskDESIWebApi.Helpers
{
    public class EmailHelper
    {
        public static void EnvioEmaiil(IEnumerable<string> para, string asunto, string mensaje, bool ssl = false, string attachment = "")
        {
            try
            {
                var de = ConfigurationManager.AppSettings["userEmail"].ToString();
                var pass = ConfigurationManager.AppSettings["passEmail"].ToString();
                var smtpURL = ConfigurationManager.AppSettings["smtpClient"].ToString();
                var puerto = Convert.ToInt32(ConfigurationManager.AppSettings["port"].ToString());

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(de, "Services Desk DESI");

                foreach (var p in para)
                {
                    mail.To.Add(p);
                }

                mail.IsBodyHtml = true;
                mail.Subject = asunto;
                mail.Body = mensaje;

                // em caso de anexos
                if (!string.IsNullOrEmpty(attachment))
                    mail.Attachments.Add(new Attachment(attachment));

                //var paraApi = new List<To>();

                //foreach (var p in para)
                //{
                //    paraApi.Add(new To()
                //    {
                //        email = p
                //    });
                //}

                //var emailApi = new SendEmail()
                //{
                //    from = new From()
                //    {
                //        email = de,
                //    },
                //    to = paraApi,
                //    subject = asunto,
                //    html_part = mensaje,
                //    text_part_auto = true,
                //};

                //HttpClientBase httpClientBase = new HttpClientBase("https://lpcorp.ipzmarketing.com/api/v1");
                //var xxx = httpClientBase.RequestAsync<string>(@"/send_emails", System.Net.Http.HttpMethod.Post, Newtonsoft.Json.JsonConvert.SerializeObject(emailApi), new Func<string, string>((strigsResponse) =>
                //{
                //    return strigsResponse;
                //}), "9_BmSBGbcshBsWas9MR3sKq4FsXYcbYpP2k78ECf").Result;

                using (var smtp = new SmtpClient(smtpURL))
                {

                    smtp.EnableSsl = true; // GMail requer SSL
                    smtp.Port = puerto;       // porta para SSL
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // modo de envio
                    smtp.UseDefaultCredentials = false; // vamos utilizar credencias especificas
                    //smtp.TargetName = "STARTTLS/smtp.office365.com";

                    // seu usuário e senha para autenticação
                    smtp.Credentials = new NetworkCredential(de, pass);

                    // envia o e-mail
                    if (para.Count() != 0)
                        smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}