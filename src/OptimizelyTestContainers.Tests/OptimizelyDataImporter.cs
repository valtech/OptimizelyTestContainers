using EPiServer.Core;
using EPiServer.Core.Transfer;
using EPiServer.Enterprise;
using Microsoft.Extensions.Logging;

namespace OptimizelyTestContainers.Tests;

public class OptimizelyDataImporter(ILogger<OptimizelyDataImporter> logger, IDataImporter dataImporter, IContentEvents contentEvents)
{
    public void Import(string importFilePath)
    {
        /*
        contentEvents.PublishedContent += (s, e) =>
        {
            logger.LogInformation("Published: {ContentName}", e.Content.Name);
        };*/

        using var stream = File.OpenRead(importFilePath);
        
        var options = new ImportOptions
        {
            KeepIdentity = true,
            EnsureContentNameUniqueness = false,
            ValidateDestination = true,
            TransferType = TypeOfTransfer.Importing,
            AutoCloseStream = true,
        };

        var importLog = dataImporter.Import(stream, ContentReference.RootPage, options);

        var errors = importLog.Errors.ToList();
        

        if (errors.Count > 0)
        {
            throw new AggregateException(errors.Select(err => new Exception(err)));
        }

        /*
         
        var warnings = importLog.Warnings.ToList();
        if (warnings.Count == 0)
        {
            return;
        }

        foreach (var warning in warnings)
        {
            logger.LogWarning(warning);
            Console.WriteLine(warning);
        }*/
    }
}