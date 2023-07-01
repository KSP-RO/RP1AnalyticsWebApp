using Microsoft.OData;
using Microsoft.OData.Edm;
using System.IO;
using System.Threading.Tasks;

namespace RP1AnalyticsWebApp.OData
{
    public class CsvOutputContext : ODataOutputContext
    {
        private Stream _stream;

        public CsvOutputContext(ODataFormat format, ODataMessageWriterSettings settings, ODataMessageInfo messageInfo)
            : base(format, messageInfo, settings)
        {
            _stream = messageInfo.MessageStream;
            Writer = new StreamWriter(_stream);
        }

        public TextWriter Writer { get; private set; }

        public override Task<ODataWriter> CreateODataResourceSetWriterAsync(IEdmEntitySetBase entitySet, IEdmStructuredType resourceType)
            => Task.FromResult<ODataWriter>(new CsvWriter(this, resourceType));

        public override Task<ODataWriter> CreateODataResourceWriterAsync(IEdmNavigationSource navigationSource, IEdmStructuredType resourceType)
            => Task.FromResult<ODataWriter>(new CsvWriter(this, resourceType));

        public void Flush() => _stream.Flush();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Writer?.Dispose();
                    _stream?.Dispose();
                }
                finally
                {
                    Writer = null;
                    _stream = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
