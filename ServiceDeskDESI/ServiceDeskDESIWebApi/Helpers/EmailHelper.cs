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

                using (var smtp = new SmtpClient(smtpURL))
                {

                    smtp.EnableSsl = true; // GMail requer SSL
                    smtp.Port = puerto;       // porta para SSL
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // modo de envio
                    smtp.UseDefaultCredentials = false; // vamos utilizar credencias especificas

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