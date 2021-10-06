using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EventLogsReport
{
    public partial class Form1 : Form
    {
        private string hostName = "" ;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            CheckEvenLogs();
        }

        private void CheckEvenLogs()
        {
            EventLog[] remoteEventLogs;
            EventLog myEventLog = new EventLog("System", ".");

            remoteEventLogs = EventLog.GetEventLogs("MININT-5LR42B8");

            richTextBox1.AppendText("Number of logs on computer: " + myEventLog.Entries.Count + " \r");

            int i = 0;
            foreach (EventLogEntry log in myEventLog.Entries)
            {
                if (i < 200 && log.EntryType.ToString().Equals("Error"))
                {
                    richTextBox1.AppendText(log.Source + " \r");
                    i++;
                }
                //Console.WriteLine("Log: " + log.Log);

            }
        }
        private void CheckTaskScheduler(string TaskName)
        {
            using (var session = new EventLogSession(hostName))
            {
                var list = GetCompletedScheduledTaskEventRecords(session, TaskName)
                    .OrderByDescending(x => x.TimeCreated)
                    .Select(r => new {   id = r.Id, level = r.LevelDisplayName,CompletedTime = r.TimeCreated, r.TaskDisplayName, Props = string.Join(" | ", r.Properties.Select(p => p.Value)) });
                for (var c = 0; c < list.Count(); c++)
                {
                    var ele = list.ElementAt(c);
                    if(ele.id != 102)
                        richTextBox1.SelectionColor = Color.Red;
                    richTextBox1.AppendText(ele.CompletedTime.ToString() + " | " + ele.TaskDisplayName + " | " + ele.level + " | " + ele.id +  " \r");
                    richTextBox1.SelectionColor = Color.Black;

                }


                //richTextBox1.dum(list);
                //.Dump("Said Tasks Completed"); //using linqpad's Dump method, this just outputs the results to the display
            }

        }

        //If you don't want completed tasks remove the second part in the where clause
        private List<EventRecord> GetCompletedScheduledTaskEventRecords(EventLogSession session, string scheduledTask)
        {
            const int TASK_COMPLETED_ID = 102;
            const int TASK_ERROR_ID = 101;
            var logquery = new EventLogQuery("Microsoft-Windows-TaskScheduler/Operational", PathType.LogName, "*[System/Level=4]") { Session = session };
            return GetRecords(logquery,
                x => x.Properties.Select(p => p.Value).Contains($@"\{scheduledTask}") && (x.Id == TASK_COMPLETED_ID || x.Id == TASK_ERROR_ID)).ToList();
        }
        private IEnumerable<EventRecord> GetRecords(EventLogQuery query, Func<EventRecord, bool> filter)
        {
            using (var reader = new EventLogReader(query))
            {
                for (var record = reader.ReadEvent(); null != record; record = reader.ReadEvent())
                {
                    if (!filter(record)) continue;

                    yield return record;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            CheckTaskScheduler(comboBox1.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hostName = System.Environment.MachineName;
            textBox1.Text = hostName;
        }
    }
}
