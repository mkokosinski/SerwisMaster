﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Xml;
using System.Threading;
using System.Windows.Documents;

namespace SerwisMaster
{
    class DodajKlienta : DaneKlienta
    {

        List<string> listaFolderów = new List<string>();
        Thread watek = null;

        public DodajKlienta(object parent)
            : base(parent)
        {
            watek = new Thread(openXmlFile);
            watek.Start();
        }

        private void openXmlFile()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(Properties.Settings.Default.baseXmlPath);
                XmlNodeList nodes = xml["Connections"].ChildNodes;
                foreach (XmlNode node in nodes)
                    listaFolderów.Add(node.Attributes["Name"].InnerText);
            }
            catch (Exception ex)
            {
                MyMessageBox.Show(ex.Message, "Błąd", MyMessageBoxButtons.Ok);
            }
        }

        protected override void zapiszKlienta()
        {
            if (sprawdzCzyKlientIstnieje() == false)
            {
                string opis = new TextRange(opisRichTextbox.Document.ContentStart, opisRichTextbox.Document.ContentEnd).Text;
                using (Klient k = new Klient(nazwaTextBox.Text, "",opis, emailList, telefonList, daneLogowaniaList,"", base.parent))
                {
                    if (parent is Folder)
                    {
                        (parent as Folder).Items.Add(k);
                    }
                    else
                    {
                        (MainWindow.listOfClients.ItemsSource as List<Element>).Add(k);
                    }

                    Serializator.serializuj(k);
                }

                MainWindow.aktualizujTreeView(MainWindow.listOfClients);

                this.Close();
            }
        }

        private bool sprawdzCzyKlientIstnieje()
        {
            bool klientJuzIstenieje = false;

            if (string.IsNullOrWhiteSpace(nazwaTextBox.Text))
            {
                klientJuzIstenieje = true;

                MessageBox.Show("Pole nazwa nie może być puste!", "Uzupełnij nazwę",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
            waitForStopingThread:
                if (watek.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    foreach (string s in listaFolderów)
                    {
                        if (s.ToLower() == nazwaTextBox.Text.ToLower().Trim())
                        {
                            klientJuzIstenieje = true;
                            MessageBox.Show("Klient o takiej nazwie już istenieje!", "Klient już istnieje",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        }
                    }
                }
                else goto waitForStopingThread;
            }
            return klientJuzIstenieje;
        }
    }
}
