# A .NET Interpreter for the Clausewitz Language
This is a modest interpreter for the Clausewitz scripting language written in C#.
The interpreter uses an abstract data tree structure when tokenizing the Clausewitz files, and offers pragma commands for sorting and indenting files.
It also enforces comment association to prevent their loss.

## Example Usage
Usage is straightforward. You can interpret a Clausewitz file as follows:
```csharp
var clause = Interpreter.InterpretText(text);
```
The interpreter recognizes the following constructs: `Clause`, `Binding`, and `Token`.
* `Clause` objects represent the abstract data tree of an interpreted Clausewitz block.
* `Binding` objects represent key-value pairs within a Clausewitz block.
* `Token` objects represent the smallest unit of meaning in a Clausewitz script, such as RGB values in a Clausewitz color block.

You can then translate the abstract data tree back to Clausewitz format:
```csharp
var text = Interpreter.TranslateClause(clause);
```

## Pragma Commands
The interpreter supports several pragma commands to manipulate Clausewitz files:
* `# [sort]`: Sorts the constructs within a clause alphabetically.
* `# [sort all]`: Sorts all constructs within a clause and its sub-clauses alphabetically.
* `# [indent]`: Indents the constructs within a clause.
* `# [indent all]`: Indents all constructs within a clause and its sub-clauses.
* `# [unindent]`: Removes indentation from the constructs within a clause.
* `# [unindent all]`: Removes indentation from all constructs within a clause and its sub-clauses.