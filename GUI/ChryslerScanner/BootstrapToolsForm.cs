﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChryslerScanner
{
    public partial class BootstrapToolsForm : Form
    {
        public MainForm OriginalForm;

        public BootstrapToolsForm(MainForm IncomingForm)
        {
            OriginalForm = IncomingForm;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MainForm.Packet.PacketReceived += PacketReceivedHandler; // subscribe to the OnPacketReceived event
            BootloaderComboBox.SelectedIndex = 0;
            WorkerFunctionComboBox.SelectedIndex = 0;
            ActiveControl = BootstrapButton;
        }

        private void SCIBusPCMBootstrapButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Turn key to OFF position." + Environment.NewLine + "Click OK when done.", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
                if (OriginalForm.PCM.speed != "62500 baud")
                {
                    OriginalForm.SelectSCIBusPCMHSMode();
                }

                MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                MainForm.Packet.tx.command = (byte)Packet.Command.settings;
                MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setProgVolt;
                MainForm.Packet.tx.payload = new byte[1] { 0x80 };
                MainForm.Packet.GeneratePacket();
                OriginalForm.TransmitUSBPacket("[<-TX] Apply VBB to SCI-RX pin:");

                if (MessageBox.Show("Turn key to RUN position." + Environment.NewLine + "Click OK when done.", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                {
                    MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                    MainForm.Packet.tx.command = (byte)Packet.Command.settings;
                    MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setProgVolt;
                    MainForm.Packet.tx.payload = new byte[1] { 0x00 };
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Remove VBB from SCI-RX pin:");

                    MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                    MainForm.Packet.tx.command = (byte)Packet.Command.debug;
                    MainForm.Packet.tx.mode = (byte)Packet.DebugMode.initBootstrapMode;
                    MainForm.Packet.tx.payload = new byte[1] { (byte)BootloaderComboBox.SelectedIndex };
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Init PCM bootstrap mode:");

                    switch ((byte)BootloaderComboBox.SelectedIndex)
                    {
                        case 0:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (128k).");
                            break;
                        case 1:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (256k).");
                            break;
                        case 2:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (custom).");
                            break;
                        default:
                            OriginalForm.UpdateUSBTextBox("[INFO] Bootloader: SBEC3 (256k).");
                            break;
                    }
                }
                else
                {
                    MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
                    MainForm.Packet.tx.command = (byte)Packet.Command.settings;
                    MainForm.Packet.tx.mode = (byte)Packet.SettingsMode.setProgVolt;
                    MainForm.Packet.tx.payload = new byte[1] { 0x00 };
                    MainForm.Packet.GeneratePacket();
                    OriginalForm.TransmitUSBPacket("[<-TX] Remove VBB from SCI-RX pin:");
                }
            }
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.usb;
            MainForm.Packet.tx.command = (byte)Packet.Command.debug;
            MainForm.Packet.tx.mode = (byte)Packet.DebugMode.writeWorkerFunction;
            MainForm.Packet.tx.payload = new byte[1] { (byte)WorkerFunctionComboBox.SelectedIndex };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Upload worker function:");

            switch ((byte)WorkerFunctionComboBox.SelectedIndex)
            {
                case 0:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: part number read.");
                    break;
                case 1:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash read.");
                    break;
                case 2:
                    OriginalForm.UpdateUSBTextBox("[INFO] Worker function: flash ID.");
                    break;
                default:
                    break;
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            MainForm.Packet.tx.bus = (byte)Packet.Bus.pcm;
            MainForm.Packet.tx.command = (byte)Packet.Command.msgTx;
            MainForm.Packet.tx.mode = (byte)Packet.MsgTxMode.single;
            MainForm.Packet.tx.payload = new byte[1] { 0x20 };
            MainForm.Packet.GeneratePacket();
            OriginalForm.TransmitUSBPacket("[<-TX] Start worker function:");
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {

        }

        private void PacketReceivedHandler(object sender, EventArgs e)
        {
            // Empty.
        }

        private void BootstrapToolsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainForm.Packet.PacketReceived -= PacketReceivedHandler; // unsubscribe from the OnPacketReceived event
        }
    }
}
