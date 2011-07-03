// tools\VisualGit.GenerateVSIXManifest\Program.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;
using VisualGit.VSPackage;

namespace VisualGit.GenerateVSIXManifest
{
    class Program
    {
        const string vsix2010 = "http://schemas.microsoft.com/developer/vsx-schema/2010";

        static void Main(string[] args)
        {
            using (FileStream fs = File.Create(args[args.Length - 1]))
            using (XmlWriter xw = XmlWriter.Create(fs, new XmlWriterSettings { Indent = true }))
            {
                xw.WriteStartElement("Vsix", vsix2010);
                xw.WriteAttributeString("Version", "1.0.0");
                AssemblyName asm = new AssemblyName(typeof(Program).Assembly.FullName);
                AssemblyName infoAsm = new AssemblyName(typeof(VisualGitPackage).Assembly.FullName);
                xw.WriteComment(string.Format("Generated by {0} {1}", asm.Name, asm.Version));

                xw.WriteStartElement("Identifier", vsix2010);
                xw.WriteAttributeString("Id", string.Format("{0}.{1}.{2}", VisualGitId.PlkProduct, VisualGitId.PlkVersion, VisualGitId.PackageId));

                xw.WriteElementString("Name", VisualGitId.ExtensionTitle);
                xw.WriteElementString("Author", VisualGitId.AssemblyCompany);
                xw.WriteElementString("Version", infoAsm.Version.ToString());
                xw.WriteElementString("Description", VisualGitId.ExtensionDescription);
                xw.WriteElementString("Locale", "1033");
                xw.WriteElementString("MoreInfoUrl", VisualGitId.ExtensionMoreInfoUrl);
                xw.WriteElementString("License", "License.rtf");
                xw.WriteElementString("GettingStartedGuide", VisualGitId.ExtensionGettingStartedUrl);
                xw.WriteElementString("Icon", VisualGitId.PlkProduct + "-Icon.png");
                xw.WriteElementString("PreviewImage", VisualGitId.PlkProduct + "-Preview.png");
                xw.WriteElementString("InstalledByMsi", "true");

                xw.WriteStartElement("SupportedProducts", vsix2010);
                xw.WriteStartElement("VisualStudio", vsix2010);
                xw.WriteAttributeString("Version", "10.0");
                xw.WriteElementString("Edition", "Ultimate");
                xw.WriteElementString("Edition", "Premium");
                xw.WriteElementString("Edition", "Pro");
                xw.WriteElementString("Edition", "IntegratedShell");
                xw.WriteEndElement();
                xw.WriteEndElement(); // /SupportedProducts

                xw.WriteStartElement("SupportedFrameworkRuntimeEdition", vsix2010);
                xw.WriteAttributeString("MinVersion", "2.0");
                xw.WriteAttributeString("MaxVersion", "4.0");
                xw.WriteEndElement(); // /SupportedFrameworkRuntimeEdition

                xw.WriteEndElement(); // /Identifier

                xw.WriteStartElement("References", vsix2010);
                xw.WriteEndElement();
                xw.WriteStartElement("Content", vsix2010);
                xw.WriteEndElement();

                xw.WriteEndElement(); // /Vsix
            }
        }
    }
}
