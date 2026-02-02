# A .NET Interpreter for the Clausewitz Language
This is a modest interpreter for the Clausewitz scripting language written in C#.
The interpreter uses an abstract data tree structure when tokenizing the Clausewitz files, and offers pragma commands for sorting and indenting files.
It also enforces comment association to prevent their loss during translation.

## Example Usage
Usage is straightforward. You can interpret a Clausewitz file as follows:
```csharp
var clause = Interpreter.InterpretText(text);
```
Provide your `text` from a Clausewitz file by reading its contents with a simple IO operation.
The interpreter recognizes the following constructs: `Clause`, `Binding`, and `Token`.
* `Clause` objects represent the abstract data tree of an interpreted Clausewitz block. It also provides the methods to manipulate the tree (see below).
* `Binding` objects represent key-value pairs within a Clausewitz block.
* `Token` objects represent the smallest unit of meaning in a Clausewitz script, such as RGB values in a Clausewitz color block.

You can then translate the abstract data tree back to Clausewitz format:
```csharp
var text = Interpreter.TranslateClause(clause);
```
You may now write the `text` back to a file using simple IO operations.

### Manipulating the Abstract Data Tree
You may search, add, and remove constructs within the abstract data tree. The `Clause` class provides the following methods:
* `CopyAllConstructsFrom(Clause source)`: Copies all constructs from the source clause to the current clause.
* `FindClauseDepthFirst(string name)`: Finds a clause by its name using depth-first search.
* `FindClauseBreadthFirst(string name)`: Finds a clause by its name using breadth-first search.
* `FindBinding(string name)`: Finds a binding by its name.
* `HasToken(string value)`: Returns true if a token exists in this clause with the specified value.
* `AddClause(string name = null)`: Adds a clause with the specified name (optional).
* `AddBinding(string name, string value)`: Adds a binding with the specified name and value.
* `AddToken(string value)`: Adds the specified value as a token.
* `RemoveClause(Clause clause)`: Removes the specified clause.
* `RemoveBinding(Binding binding)`: Removes the specified binding.
* `RemoveToken(Token token)`: Removes the specified token.

You may also sort the current clause's constructs alphabetically using `Sort()`.
You may not alter the readonly collections directly (`Constructs`, `Clauses`, `Bindings`, `Tokens`). They are provided for loops.
To modify the abstract data tree, use the provided methods.

**Note:** Recall that in Clausewitz files, multiple constructs can share the same name within a single clause, such as scopes and logical operators.

## Pragma Commands
The interpreter supports several pragma commands (in comments) to manipulate Clausewitz files.
You can use them by placing them in comments within your Clausewitz files.
* `# [sort]`: Sorts the constructs within a clause alphabetically.
* `# [sort all]`: Sorts all constructs within a clause and its sub-clauses alphabetically.
* `# [indent]`: Indents the constructs within a clause.
* `# [indent all]`: Indents all constructs within a clause and its sub-clauses.
* `# [unindent]`: Removes indentation from the constructs within a clause.
* `# [unindent all]`: Removes indentation from all constructs within a clause and its sub-clauses.