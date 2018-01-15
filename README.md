# WolframCarbid

Welcome to Wolframcarbid!
 
As a Windows C/C++ developer for more than 15 years, I rarely have opportunities to develop C# programs. Therefore I begin to forge this afterwork side project, and it's intended to enrich my implementation experiences in term of C# language skills, Common Language Runtime knoweldges and .NET class libraries familiarity.
 
The common "Hello World" project is too amatuer to be a start up project, thus I simply rebuild some of my frequently used design patterns and application model in this C# project.
 
This **“Wolframcarbid”** project is a command-line interface application, and it provides a command handler model and master-slave architecture. I call this project "Wolframcarbid" (it's a German vocabulary and it means Tungsten Carbide) because the original C++ project is called "Wolfram" and this "Wolframcarbid" is conceptually derived from “Wolfram”.
 
At this point, though there are something I still want to forge, this project has roughly accomplished some main feature sets I plan to deploy. Before I drill down the technical characteristics, I prefer to note some C# development experiences by comparing with C/C++.

## C/C++ vs. C#
### Learning Curve
Just like many programming language forums have mentioned, the C# learning curve is much mild than C/C++.
To be a qualified Windows C/C++ programmer, the beginners not only have to understand basic C/C++syntax and fundamental object-oriented concept, they also have to understand CPU architecture, memory management, Windows platform characteristics, or even C/C++ compiler and linker design concepts. After a few year struggles, possibly newbies are capable of building a sustainable and low defect density application.

On the contray, C# beginner only has to worry about object-oriented design and relative models, maybe plus a bit call by reference concepts. With strong support of .NET class libraries, even beginners can build up a fancy Windows application with C#.

### Implementation Philosophy
Another interesting differences between C/C++ and C# is a implementation philosophy. In this project, it offers a self-installing capability. In Windows C/C++ implementation, calling several Win32 API's can easily achieve this job. However, there is no such low-level API to use and I’ve wasted quite a long time to search and study those secret C# API. Finally, I realize that I shouldn’t think of C# as a low-level machine, I should elaborate C# codes in a more human nature fashion. At that moment, I felt totally comfortable to use ManagedInstallerClass(), just like what InstallUtil.exe will do.

### Singleton?
In addition, there is a debatable issue for singleton design pattern. In C++ programming, the singleton is a good approach to ensure single instance. However, since C# provide static class globally, maybe singleton in C# is a bad smell. Honestly I’m not quite sure about it, just leave a note here.

## Features and Design Models
Now let’s get back to technical factors. The Wolframcarbid provides following facilities and design models which are frequently used in my programming career.

### Command-Line Interface
It defines a form and provides a fundamental syntax parser. The parameters can be mandatory or optional.
```
Wolframcarbid.exe -wc=cmd_name -param1=value1 -param2=value2
```

### Self-installing Windows Service
By command “install”, Wolframcarbid.exe can install itself as a Windows System Service. The Wolframcarbid.exe is capable of operating tasks in long term duration either periodically or event driven.

### Command Handler
Command Hanlder is derived from famouse CQRS (Command Query Role Separation) model. In Wolframcarbid, every command are managed by a delegated command handler. It highly concentrates specific business logics into a single handler class. This allow developers to effectively insert a new command or remove an obsolete command. 

### Master/Slave Model
For every daemon service, inevitably, the resource leak is the biggest pain and suffering. The msater/slave is the one of the solutions to reduce potential leaks. Take Windows IIS as example, the IIS service doesn’t handle any incoming request in its own regime. Instead, it forks at least one w3wp.exe worker process to handle HTTP requests. Wolframcarbid deploys the similar strategy, the Wolframcarbid service doesn’t manage tasks by itself, it forks Wolframcarbid worker process to manage any request. This model keeps the Wolframcarbid to be resource leak free for a very long duration.

Richard, Chih-Yao Sun
Taipei (台北)


