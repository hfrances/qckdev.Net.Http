[![NuGet Version](https://img.shields.io/nuget/v/qckdev.Net.Http.svg)](https://www.nuget.org/packages/qckdev.Net.Http)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=qckdev.Net.Http&metric=alert_status)](https://sonarcloud.io/dashboard?id=qckdev.Net.Http)
[![Code Coverage](https://sonarcloud.io/api/project_badges/measure?project=qckdev.Net.Http&metric=coverage)](https://sonarcloud.io/dashboard?id=qckdev.Net.Http)
![Azure Pipelines Status](https://hfrances.visualstudio.com/Main/_apis/build/status/qckdev.Net.Http?branchName=master)


# qckdev.Net.Http

Provides extensions to **System.Net.Http** namespace.

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

## 🤝 Contributing
Issues and pull requests are welcome! See the contribution guidelines (coming soon).

## 📜 License
This project is licensed under the terms of the [MIT License](LICENSE).
