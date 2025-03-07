﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace PrBoomRecordTool
{
    class DemoRecorder
    {
        private const string PATH_NOT_SET = "You haven't probably set up path to your prboom/glboom. Set this path to be able to record and play demos.";
        private const string PROCESS_IN_PROGRESS = "PrBoom/GlBoom instance is still probably running hence you can't record/play demos at this moment.";
        private const string INVALID_PATH = "PrBoom/GlBoom not found, try to relocate your sourceport.";
        private const string UNSPECIFIED_ERROR = "Couldn't initiate PrBoom/GlBoom";

        private Process currentProcess;

        private readonly App app;

        public DemoRecorder(App app)
        {
            this.app = app;
        }

        public void StartRecording()
        {
            if (!IsRecordToolReady()) 
            {
                return;
            }

            try
            {
                File.Delete(Config.GetLastDemoName());
            }
            catch {}
           
            RunProcess(GetRecordArguments());
        }

        public void PlayRecording()
        {
            if (!IsRecordToolReady())
            {
                return;
            }

            RunProcess(GetPlayArguments());
        }

        private bool IsRecordToolReady()
        {
            bool isReady = Config.GetPrBoomPath().Length != 0;

            if (!isReady) 
            {
                ShowWarningMessage(PATH_NOT_SET);
            }

            isReady = currentProcess == null || currentProcess.HasExited;

            if (!isReady)
            {
                ShowWarningMessage(PROCESS_IN_PROGRESS);
            }

            return isReady;
        }

        private DialogResult ShowWarningMessage(string content, string title = "Something went wrong")
        {
            return MessageBox.Show(content.Trim(), title.Trim(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private string GetPlayArguments()
        {
            return $@"
                -iwad {Config.GetIwadPath()}
                -playdemo {Config.GetLastDemoName()}
                -complevel {app.complevelInput.Value}
                {GetFileParameter()}
            ";
        }

        private string GetRecordArguments() 
        {
            string episode = app.episodeInput.Enabled ? app.episodeInput.Value.ToString() : "";
            string noMonsters = app.noMonstersCheckbox.Checked ? "-nomonsters" : "";
            string fastMonsters = app.fastMonstersCheckbox.Checked ? "-fast" : "";
            string respawn = app.respawnCheckbox.Checked ? "-respawn" : "";

            return $@"
                -iwad {Config.GetIwadPath()}
                -record {Config.GetLastDemoName()}
                -warp {episode} {app.levelInput.Value}
                -complevel {app.complevelInput.Value}
                -skill {app.skillSelect.SelectedIndex + 1}
                {GetFileParameter()}
                {noMonsters}
                {fastMonsters}
                {respawn}
            ";
        }

        private string GetFileParameter()
        {
            string pwadList = string.Join(" ", Config.GetPwads());

            return pwadList.Length > 0 ? $"-file {pwadList}" : "";
        }

        private void RunProcess(string arguments)
        {
            try
            {
                currentProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Config.GetPrBoomPath(),
                        Arguments = arguments,
                        CreateNoWindow = true
                    },
                };

                currentProcess.Start();
            }
            catch (Win32Exception)
            {
                currentProcess = null;
                ShowWarningMessage(INVALID_PATH);
            }
            catch (Exception e)
            {
                currentProcess = null;
                ShowWarningMessage($"{UNSPECIFIED_ERROR}: {e.Message}");
            }
        }
    }
}
