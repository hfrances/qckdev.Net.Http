<a href="https://www.nuget.org/packages/qckdev.Net.Http"><img src="https://img.shields.io/nuget/v/qckdev.Net.Http.svg" alt="NuGet Version"/></a>
<a href="https://sonarcloud.io/dashboard?id=qckdev.Net.Http"><img src="https://sonarcloud.io/api/project_badges/measure?project=qckdev.Net.Http&metric=alert_status" alt="Quality Gate"/></a>
<a href="https://sonarcloud.io/dashboard?id=qckdev.Net.Http"><img src="https://sonarcloud.io/api/project_badges/measure?project=qckdev.Net.Http&metric=coverage" alt="Code Coverage"/></a>
<a><img src="https://hfrances.visualstudio.com/Main/_apis/build/status/qckdev.Net.Http?branchName=main" alt="Azure Pipelines Status"/></a>


# qckdev.Net.Http

```cs
using System;	

namespace Entities
{
	sealed class Species
	{
		public string Name { get; set; }
		public string Url { get; set; }
	}

	sealed class Pokemon
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Order { get; set; }

		public Species Species { get; set; }

	}
}
```

```cs
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using qckdev.Net.Http;

const string URL = "https://pokeapi.co/api/v2/";

using (var client = new HttpClient() { BaseAddress = new Uri(URL) })
{
	Entities.Pokemon rdo;

	rdo = await client.Fetch<Entities.Pokemon>(HttpMethod.Get, "pokemon/ditto");
}
```

## Example for enumerations

```cs

enum Visibility {
  Visible,
  Hidden  
}

sealed class Example {

  public int Id { get; set; }

  // Choose one of these attributes depending on the targeting framework.
  [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
  [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
  public Visibility Visibility { get; set; }

}

```