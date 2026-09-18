using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Khonshu.Common;


public sealed class ReadOnlyConfig
{
    private readonly IConfigurationRoot config;
    private IConfigurationSection? section;
    
    
        
    public ReadOnlyConfig(String configFilePath, String? mainSection = null)
    {
        config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(configFilePath, optional: false, reloadOnChange: true).Build();
        if (!String.IsNullOrEmpty(mainSection))
            section = config.GetSection(mainSection);
        else
            section = null;
    }

    public ReadOnlyConfig(IConfigurationRoot config)
    {
        this.config = config;
    }

    public String? GetSection(String sectionName)
    {
        if(section is null)
            return config.GetSection(sectionName).Value;
        
        return section.GetSection(sectionName).Value;
        
    }

    public override String ToString()
    {
        return "Config";
    }

    
}