# RoslynPad.VB

A cross-platform VB editor based on Roslyn and AvalonEdit.

It's a modified version of [roslynpad](https://github.com/roslynpad/roslynpad) that replaces every C# compiler usages with the use of VB compiler.

## Progress
- [x] Code Fixes
- [x] Diagnostics
- [x] Tool tips
- [ ] Completion
    - [x] Show completion list
    - [ ] Align when to show the completion list with Visual Studio 2022 community edition
    - [ ] Insert end statements when you press `Enter`
- [ ] VB key binding (such as use `Ctrl+J` to trigger completion)
- [ ] Compile and run user's code
- [ ] Convert the source code of this project from C# to VB as much as possible
- [ ] Publish NuGet package
- [ ] Replace assets
- [ ] Publish to store
- [ ] VB Integrated Scripting Environment (ISE)
- [ ] Add AI code completion (it's being prototyped in a private repo)
    - [ ] Line completion
    - [ ] Block completion
    - [ ] Chat panel with quick actions and RAG
    - [ ] Model providers
        - [ ] In process (e.g. LlamaSharp)
        - [ ] Local service (e.g. Ollama)
        - [ ] Remote service (e.g. OpenAI, TongYi)
