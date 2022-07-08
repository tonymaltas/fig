﻿using System.Collections.Generic;

namespace Fig.Contracts.ImportExport
{
    public class PluginVerificationExportDataContract
    {
        public PluginVerificationExportDataContract(string name, IList<string>? propertyArguments)
        {
            Name = name;
            PropertyArguments = propertyArguments;
        }

        public string Name { get; set; }

        public IList<string>? PropertyArguments { get; set; }
    }
}