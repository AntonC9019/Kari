// Template file generated by Baton. Feel free to change it or remove this message.
using System.Threading.Tasks;
using Kari.GeneratorCore.Workflow;
using Kari.Utils;

namespace Kari.Plugins.DataObject
{
    // The plugin interface to Kari.
    // Kari will call methods of this class to analyze and then generate code.
    public class DataObjectAdministrator : IAdministrator
    {
        public DataObjectAnalyzer[] _analyzers;
        
        public void Initialize()
        {
            AdministratorHelpers.Initialize(ref _analyzers);
            DataObjectSymbols.Initialize(new NamedLogger("DataObject"));
        }
        
        public Task Collect()
        {
            return AdministratorHelpers.CollectAsync(_analyzers);
        }

        public Task Generate()
        {
            return Task.WhenAll(
                AdministratorHelpers.GenerateAsync(_analyzers, "DataObjects.cs"),
                AdministratorHelpers.AddCodeStringAsync(
                    MasterEnvironment.Instance.CommonPseudoProject,
                    "DataObjectAnnotations.cs", nameof(DataObjectAnalyzer), DummyDataObjectAnnotations.Text)
            );
        }
        
        public string GetAnnotations() => DummyDataObjectAnnotations.Text;
    }
}
