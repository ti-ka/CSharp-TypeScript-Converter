# CSharp-TypeScript-Converter
A Simple C# to TypeScript Converter

This utility will convert C# Models into TypeScript classes:

```
var configuration = new Configuration();
configuration.OutputDirectory = 'output-ts';

// The Models Assembly which is to be exported
configuration.Assembly = Assembly.GetAssembly(typeof(BaseEntity));

var engine = new Engine(configuration);
engine.Export();

```



If you wish to be maintainer of this project, email tika.pahadi@selu.edu.
