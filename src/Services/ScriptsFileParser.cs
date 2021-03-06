using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ivanize.DotnetTool.Exec
{
    public class ScriptsFileParser : IScriptsFileParser
    {
        private IDefaultEntrypointDetector defaultEntrypointDetector;

        public ScriptsFileParser(IDefaultEntrypointDetector defaultEntrypointDetector)
        {
            this.defaultEntrypointDetector = defaultEntrypointDetector ?? throw new ArgumentNullException(nameof(defaultEntrypointDetector));
        }

        public Package Parse(StreamReader scriptsFileStream)
        {
            if (scriptsFileStream == null)
                throw new ArgumentNullException(nameof(scriptsFileStream));

            var content = scriptsFileStream.ReadToEnd();

            InternalPackage pkgInstance = null;
            try
            {
                pkgInstance =
                    JsonConvert.DeserializeObject<InternalPackage>(content);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Unable to parse the scripts file!", ex);
            }

            if (pkgInstance == null) throw new InvalidDataException("Unable to parse the scripts file!");
            if (String.IsNullOrWhiteSpace(pkgInstance.Name)) throw new InvalidDataException("The 'name' property is required!");


            if (pkgInstance.Env.Any(s => string.IsNullOrWhiteSpace(s.Key))) throw new InvalidDataException("The Variable `name` is required!");
            if (pkgInstance.Commands.Any(s => string.IsNullOrWhiteSpace(s.Key))) throw new InvalidDataException("The Command `name` is required!");


            var pkg = new Package(
                pkgInstance.Name,
                pkgInstance.Entrypoint ?? this.defaultEntrypointDetector.GetDefaultEntryPoint(),
                pkgInstance.Env.Select(s => new EnvVariable(s.Key, s.Value)).ToArray(),
                pkgInstance.Commands.Select(s => new Command(s.Key, s.Value)).ToArray());

            return pkg;
        }
        // Internal classes
        private class InternalPackage
        {
            public string Name { get; set; }
            public string Entrypoint { get; set; }
            public Dictionary<string, string> Env { get; set; }
            public Dictionary<string, string[]> Commands { get; set; }

            public InternalPackage()
            {
                this.Env = new Dictionary<string, string>();
                this.Commands = new Dictionary<string, string[]>();
            }
        }
    }
}
