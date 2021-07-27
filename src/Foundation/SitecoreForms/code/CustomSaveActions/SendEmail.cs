namespace Sitecore.Foundation.SitecoreForms.CustomSaveActions
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using Sitecore.Data;
    using Sitecore.Diagnostics;
    using Sitecore.ExperienceForms.Models;
    using Sitecore.ExperienceForms.Mvc.Models.Fields;
    using Sitecore.ExperienceForms.Processing;
    using Sitecore.ExperienceForms.Processing.Actions;
    using Sitecore.Foundation.SitecoreForms.Models;

    /// <summary>
    /// Sitecore forms custom save action for email sending
    /// </summary>
    public class SendEmail : SubmitActionBase<SendEmailActionData>
    {
        public SendEmail(ISubmitActionData submitActionData) : base(submitActionData)
        { }
        /// <summary>
        /// Send email custom save action functionalities
        /// </summary>
        /// <param name="data"></param>
        /// <param name="formSubmitContext"></param>
        /// <returns></returns>
        protected override bool Execute(SendEmailActionData data, FormSubmitContext formSubmitContext)
        {
            try
            {
                var emailTemplate = Sitecore.Context.Database.GetItem(new ID(data.ReferenceId));

                //Replace keywords in 'Subject' from form fields
                var emailSubject = ReplaceKeywords(emailTemplate[Templates.FormEmail.Subject], formSubmitContext);

                //Replace 'From' email address from form fields
                var fromEmailAddress = ReplaceKeywords(emailTemplate[Templates.FormEmail.From], formSubmitContext);

                //Replace keywords in 'FromDisplayName' from form fields
                var fromDisplayName = ReplaceKeywords(emailTemplate[Templates.FormEmail.FromDisplayName], formSubmitContext);

                //Replace 'TO' email addresses from form fields                
                var toEmailAddresses = ReplaceKeywords(emailTemplate[Templates.FormEmail.To], formSubmitContext);

                //Replace 'CC' email addresses from form fields                
                var ccEmailAddresses = ReplaceKeywords(emailTemplate[Templates.FormEmail.CC], formSubmitContext);

                //Replace 'BCC' email addresses from form fields                
                var bccEmailAddresses = ReplaceKeywords(emailTemplate[Templates.FormEmail.BCC], formSubmitContext);

                //Replace email message body from form fields
                var message = ReplaceKeywords(emailTemplate[Templates.FormEmail.Body], formSubmitContext);

                this.Send(fromEmailAddress, fromDisplayName, toEmailAddresses, ccEmailAddresses, bccEmailAddresses, emailSubject, message, true);
                Log.Debug(string.Format("Email sent with following details for form id: {0}: Subject- {1} | FromAddress - {2} | FromDisplyName - {3} | ToAddress - {4} | CCAddress - {5} | BCCAddress - {6}",
                    formSubmitContext.FormId.ToString(), emailSubject, fromEmailAddress, fromDisplayName, toEmailAddresses, ccEmailAddresses, bccEmailAddresses), this);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Exception occured executing send email custom save action for form id: {0}.", formSubmitContext.FormId.ToString()), ex, this);
                return false;
            }
        }

        /// <summary>
        /// Replace keywords from the form input data
        /// </summary>
        /// <param name="original"></param>
        /// <param name="formSubmitContext"></param>
        /// <returns></returns>
        protected string ReplaceKeywords(string original, FormSubmitContext formSubmitContext)
        {
            var returnString = original;
            foreach (var viewModel in formSubmitContext.Fields)
            {
                if (returnString.Contains("{" + viewModel.Name + "}"))
                {
                    var type = viewModel.GetType();
                    string valueToReplace = string.Empty;

                    // InputViewModel<string> types
                    if (type.IsSubclassOf(typeof(InputViewModel<string>)))
                    {
                        var field = (InputViewModel<string>)viewModel;
                        valueToReplace = field.Value ?? string.Empty; ;
                    }
                    // InputViewModel<List<string>> types
                    else if (type.IsSubclassOf(typeof(InputViewModel<List<string>>)))
                    {
                        var field = (InputViewModel<List<string>>)viewModel;
                        valueToReplace = (field.Value != null) ? string.Join(", ", field.Value) : string.Empty;
                    }
                    // InputViewModel<bool> types
                    else if (type.IsSubclassOf(typeof(InputViewModel<bool>)))
                    {
                        var field = (InputViewModel<bool>)viewModel;
                        valueToReplace = field.Value.ToString();
                    }
                    // InputViewModel<DateTime?> types
                    else if (type.IsSubclassOf(typeof(InputViewModel<DateTime?>)))
                    {
                        var field = (InputViewModel<DateTime?>)viewModel;
                        valueToReplace = field.Value?.ToString() ?? string.Empty;
                    }
                    // InputViewModel<DateTime> types
                    else if (type.IsSubclassOf(typeof(InputViewModel<DateTime>)))
                    {
                        var field = (InputViewModel<DateTime>)viewModel;
                        valueToReplace = field.Value.ToString();
                    }
                    // InputViewModel<double?> types
                    else if (type.IsSubclassOf(typeof(InputViewModel<double?>)))
                    {
                        var field = (InputViewModel<double?>)viewModel;
                        valueToReplace = field.Value?.ToString() ?? string.Empty;
                    }

                    returnString = returnString.Replace("{" + viewModel.Name + "}", valueToReplace);
                }
            }

            return returnString;
        }

        /// <summary>
        /// Send the email based to parameters
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="fromName"></param>
        /// <param name="toAddresses"></param>
        /// <param name="ccAddresses"></param>
        /// <param name="bccAddresses"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="isHtml"></param>
        private void Send(string fromAddress, string fromName, string toAddresses, string ccAddresses, string bccAddresses, string subject, string message, bool isHtml)
        {
            var mailMessage = new MailMessage();

            mailMessage.From = new MailAddress(fromAddress, fromName);

            var toAddressList = toAddresses.Split(',');
            foreach (var addressItem in toAddressList)
            {
                if (!string.IsNullOrEmpty(addressItem))
                {
                    mailMessage.To.Add(new MailAddress(addressItem));
                }
            }

            var ccAddressList = ccAddresses.Split(',');
            foreach (var addressItem in ccAddressList)
            {
                if (!string.IsNullOrEmpty(addressItem))
                {
                    mailMessage.CC.Add(new MailAddress(addressItem));
                }
            }

            var bccAddressList = bccAddresses.Split(',');
            foreach (var addressItem in bccAddressList)
            {
                if (!string.IsNullOrEmpty(addressItem))
                {
                    mailMessage.Bcc.Add(new MailAddress(addressItem));
                }
            }

            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = isHtml;
            mailMessage.Body = message;
            MainUtil.SendMail(mailMessage);
        }
    }
}
