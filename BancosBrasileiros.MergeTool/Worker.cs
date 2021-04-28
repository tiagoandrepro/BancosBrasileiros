﻿// ***********************************************************************
// Assembly         : BancosBrasileiros.MergeTool
// Author           : Guilherme Branco Stracini
// Created          : 19/05/2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 04-27-2021
// ***********************************************************************
// <copyright file="Worker.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace BancosBrasileiros.MergeTool
{
    using Helpers;
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Class Worker.
    /// </summary>
    internal class Worker
    {

        /// <summary>
        /// Works this instance.
        /// </summary>
        public void Work()
        {
            Console.WriteLine("Reading data files");

            var reader = new Reader();

            var csv = reader.LoadCsv();
            var html = reader.LoadHtml();
            var json = reader.LoadJson();
            var markdown = reader.LoadMarkdown();
            var sql = reader.LoadSql();
            var xml = reader.LoadXml();

            Console.WriteLine($"CSV: {csv.Count} banks | HTML: {html.Count} | JSON: {json.Count} banks | SQL: {sql.Count} banks | Markdown: {markdown.Count} banks | XML: {xml.Count} banks");

            var comparer = new Comparer();
            var normalized = comparer.Compare(csv, html, json, markdown, sql, xml);

            Console.WriteLine($"Normalized: {normalized.Count} banks");

            var cnpj = reader.LoadCnpj();
            var ispb = reader.LoadIspb();
            var site = reader.LoadSite();
            var slc = reader.LoadSlc();

            Console.WriteLine($"CNPJ: {cnpj.Count} banks | ISPB: {ispb.Count} banks | Site: {site.Count} banks | SLC: {slc.Count} banks");

            var seeder = new Seeder(); //The order must be Site -> ISPB -> CNPJ -> SLC to find the Fiscal Name and de COMPE code.
            seeder.SeedSite(normalized, site);
            seeder.SeedIspb(normalized, ispb);
            seeder.SeedDocument(normalized, cnpj);
            seeder.SeedSlc(normalized, slc);

            foreach (var bank in normalized)
            {
                bank.DateRegistered ??= DateTimeOffset.Now;
                bank.DateUpdated ??= DateTimeOffset.Now;
            }

            Console.WriteLine("Saving result files");

            var writer = new Writer();
            writer.Save(normalized);

            Console.WriteLine("Merge done. Check 'result' folder in 'bin' directory!");

            var binDirectory = Directory.GetCurrentDirectory();
            var resultDirectory = Path.Combine(binDirectory, "result");
            Process.Start("explorer.exe", resultDirectory);
        }
    }
}
