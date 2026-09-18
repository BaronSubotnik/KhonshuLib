using System;
using System.Collections.Generic;
using System.Text;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Text.RegularExpressions;

namespace Khonshu.Otpavitel
{
    public sealed class EmailKhonshu
    {
        private readonly String Email;
        private readonly String Name;
        private readonly String password;
    
        public EmailKhonshu(String Email, String Name, String password)
        {
            this .Email = Email;
            this.Name = Name;
            this.password = password;
        }

        public static Boolean isGmail(String emailAddr)
        { 
            String pattern = @"^[a-zA-Z0-9._%+-]+@gmail\.com$";

            return Regex.IsMatch(emailAddr, pattern, RegexOptions.IgnoreCase);


        }

        public  async Task SendEmailMessageAsync(String AddressPolychatela, String subject, String bodyHTML)
        {
            try
            {
                var Message = new MimeMessage();

                Message.From.Add(new MailboxAddress(Name, Email));
                Message.To.Add(new MailboxAddress("", AddressPolychatela));


                Message.Subject = subject;


                var builder = new BodyBuilder();
                builder.HtmlBody = bodyHTML;
                builder.TextBody = "Вы не поддерживаете HTML";

                Message.Body = builder.ToMessageBody();


                using var client = new SmtpClient(new ProtocolLogger("smpt.log")) ;
                try
                {
                    try
                    {
                        await client.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    }
                    catch (System.TimeoutException)
                    {
                        await client.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                    }
                    catch (MailKit.Net.Smtp.SmtpProtocolException)
                    {
                        await client.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                    }


                    await client.AuthenticateAsync(Email, password);
                    await client.SendAsync(Message);
                    await client.DisconnectAsync(true);
                }
                finally
                {
                    client.Dispose();
                }
                
            }
            catch(Exception ex) 
            {
                Console.WriteLine($"{ex} \n\n {ex.Message}");
            }
            


            
        }
    }
}
