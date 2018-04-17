using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace safeprojectname.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            string ConnString = @"Data Source=localhost;Initial Catalog=test;Integrated Security=True;MultipleActiveResultSets=true";
            string SqlString = $"select answer from [Table] where question like '%{activity.Text}%'";
            string claimId="";
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                using (SqlCommand cmd = new SqlCommand(SqlString, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("question", activity.Text);

                    

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                claimId = reader["answer"].ToString();
                                
                                //Thread.Sleep(2000);
                                //reader.Close();
                            }
                        }
                        
                    }
                    if (claimId == "") { claimId = "Sorry input invalid"; }

                }
            }

            // return our reply to the user
            await context.PostAsync($"{claimId}");

            context.Wait(MessageReceivedAsync);
        }
    }
}