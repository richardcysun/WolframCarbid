# WolframCarbid

Welcome to Wolframcarbid!
 
As a Windows C/C++ developer for more than 15 years, I rarely have opportunities to develop C# programs. Therefore I begin to forge this afterwork side project, and it's intended to enrich my implementation experiences in term of C# language skills, Common Language Runtime knoweldges and .NET class libraries familiarity.
 
The common "Hello World" project is too amatuer to be a start up project, thus I simply rebuild some of my frequently used design patterns and application model in this C# project. Furthermore, I also leverage this project as a C# sample codes repository, not only for self-use, but also a feedback to programming community.
 
This **“Wolframcarbid”** project is a command-line interface application, and it provides a command handler model and master-slave architecture. I call this project "Wolframcarbid" (it's a German vocabulary and it means Tungsten Carbide) because the original C++ project is called "Wolfram" and this "Wolframcarbid" is conceptually derived from “Wolfram”.
 
At this point, though there are something I still want to forge, this project has roughly accomplished some main feature sets I plan to deploy. Before I drill down the technical characteristics, I prefer to note some C# development experiences by comparing with C/C++.

在Windows 世界裡，用 C/C++ 耕田種地十餘年，一直苦無機會撰寫 C# 程式。最近於工作之餘，開發了這個專案，想要藉此體會一下 C# 實作心得，順便了解 Common Language Runtime 以及 .NET 類別庫。

尋常 "Hello World" 無太多可著墨之處， 所以敝人將一些工作上常用的開發樣板以及模型，以 C# 具體實踐。某種程度而言，也把這個專案當成是 C# Sample Codes 的累積，以備將來不時之需，同時也提供給社群分享。

Wolframcarbid 專案是個提供命令列框架的工具程式，它同時也具備了 Command Handler 模型及 Master/Slave 架構。稱本專案為 Wolframcarbid (德文，碳化鎢) 是因為其概念衍生自本人使用的另個 C++ 專案 Wolfram (德文，鎢)。 

此時雖然還有一些想作但未完成的實作，不過具體框架也算粗略完成。不過在介紹技術特徵前，還是先來說一下 C/C++ 與  C# 的相異之處。

## C/C++ vs. C#
### Learning Curve
Just like many programming language forums have mentioned, the C# learning curve is much mild than C/C++.
To be a qualified Windows C/C++ programmer, the beginners not only have to understand basic C/C++syntax and fundamental object-oriented concept, they also have to understand CPU architecture, memory management, Windows platform characteristics, or even C/C++ compiler and linker design concepts. After a few year struggles, possibly newbies are capable of building a sustainable and low defect density application.

On the contray, C# beginner only has to worry about object-oriented design and relative models, maybe plus a bit call by reference concepts. With strong support of .NET class libraries, even beginners can build up a fancy Windows application with C#.

如同眾多語言論壇所言，C# 的學習曲線，確實比 C/C++ 和緩許多。要成為一位稱職的 C/C++ 寫手，除了基本語法理解及物件導向觀念外，對於處理器架構，記憶體管理，Windows 平台特徵，甚至 C/C++ 的編譯器及連結器設計概念，都要有相當理解。過了兩三年之後，才有機會寫出一套可長期運作，而且臭蟲稍少的程式。

另一方面， C# 的寫手確實只要面對物件導向的設計規劃，外帶一些對於 call-by-reference 的理解即可。在 .NET 程式庫的支援下，真的可以比較容易發展出一套商業運轉軟體。

### Implementation Philosophy
Another interesting differences between C/C++ and C# is a implementation philosophy. In this project, it offers a self-installing capability. In Windows C/C++ implementation, calling several Win32 API's can easily achieve this job. However, there is no such low-level API to use and I’ve wasted quite a long time to search and study those secret C# API. Finally, I realize that I shouldn’t think of C# as a low-level machine, I should elaborate C# codes in a more human nature fashion. At that moment, I felt totally comfortable to use ManagedInstallerClass(), just like what InstallUtil.exe will do.

C/C++ 與 C# 在實作心態上，的確大不相同。C/C++ 寫久了，碰到問題就很習慣性的往底層鑽。以專案中的自我安裝功能為例，用 C/C++開發，只要若干個 Win32 API 兜在一起，就可以在簡短時間內完成。

可是 C# 不提供這麼底層的類別庫，當初為了找到 C# 實作的底層方法，著實花了不少時間。後來看了不少討論，才理解到 C# 開發要以人類自然思維為主，才找到如同與 InstallUtil.exe 相同的 ManagedInstallerClass()。

### Singleton?
In addition, there is a debatable issue for singleton design pattern. In C++ programming, the singleton is a good approach to ensure single instance. However, since C# provide static class globally, maybe singleton in C# is a bad smell. Honestly I’m not quite sure about it, just leave a note here.

這一點比較有爭議些，不過還是列下來。Singleton 是個在 C++ 上面常用，確保 instance 唯一性的模型，但是在 C# 中，有了 static class，Singleton 在有存在意義嗎?

## Features and Design Models
Now let’s get back to technical factors. The Wolframcarbid provides following design models which are frequently used in my programming career. In addition, I also forge some C# examples and related .NET classes in this list. 

以下介紹在 Wolframcarbid 專案中，個人於工作中常使用的模型。另外也註記相關 Example 以及使用到的 .NET Class。

### Command-Line Interface
It defines a form and provides a fundamental syntax parser. The parameters can be mandatory or optional. There are two types of commands, self-sustained or master/slave model. Self-sustained command is managed by single Wolframcarbid process. As for master/slave model, it involves  Wolframcarbid  command invoker, Wolframcarbid system service and Wolframcarbid worker and it can do business in system privilege. Will explain more details in Master/Slave section.

提供命令列的語法檢查，命令參數可以是選擇或強制的。命令性質有兩種，自給自足型或是主人奴隸型。自給自足型的命令可在單一 Wolframcarbid 完成所有工作。主人奴隸型命令則仰賴 Wolframcarbid  command invoker/Wolframcarbid system service/Wolframcarbid worker 三者合作，可以系統權限工作。

```
Wolframcarbid.exe -wc=cmd_name -param1=value1 -param2=value2
```
:notebook: Dictionary<TKey, TValue> Class

### Self-installing Windows Service
By command “install”, Wolframcarbid.exe can install itself as a Windows System Service. The Wolframcarbid.exe is capable of operating tasks in long term duration either periodically or event driven.

可自我安裝成 Windows System Service，提供長效型的工作平台。

```
Wolframcarbid.exe -wc=inst
```
:notebook: Installer Class
:notebook: ServiceProcessInstaller Class
:notebook: ServiceInstaller Class
:notebook: RegistryKey Class

### Command Handler
Command Hanlder is derived from famouse CQRS (Command Query Role Separation) model. In Wolframcarbid, every command are managed by a delegated command handler. It highly concentrates specific business logics into a single handler class. This allow developers to effectively insert a new command or remove an obsolete command. 

建構發源自 CQRS 概念的 Command Handler，此處用途雖然與 CQRS 的初衷頗有不同，不過此模型可以確保每種命令與對應處理元件都是分工明確的，維持整體架構的簡潔與優雅。

:notebook: Dictionary<TKey, TValue> Class

### Master/Slave Model
For every daemon service, inevitably, the resource leak is the biggest pain and suffering. The msater/slave is the one of the solutions to reduce potential leaks. Take Windows IIS as example, the IIS service doesn’t handle any incoming request in its own regime. Instead, it forks at least one w3wp.exe worker process to handle HTTP requests. Wolframcarbid deploys the similar strategy, the Wolframcarbid service doesn’t manage tasks by itself, it forks Wolframcarbid worker process to manage any request. This model keeps the Wolframcarbid to be resource leak free for a very long duration.

只要是常駐型的系統，運行時日一久難免面臨資源洩漏問題。以 IIS 為例，解決方案之一就是派出  工人 w3wp.exe 應對服務需求。Wolframcarbid 也提供類似的模型，凡是需要 Wolframcarbid Service 協助的命令，均由 Wolframcarbid Worker 出面打發。

:notebook: Process Class
:notebook: ProcessStartInfo Class
### AES Encryption
There is a small class wrapper combines AesCryptoServiceProvider and Base64 to convert plaintext and ciphertext.

整合 AesCryptoServiceProvider 以及 Base64，提供一個簡單的加解密類別。

:notebook: AesCryptoServiceProvider Class
### Task Scheduler
As everyone mentions Task Scheduler, people always think of TaskScheduler and TaskDefinition classes. But I find they are a bit heavy in this project. Therefore I simply use a simple Timer to achieve the same objective.

在 C# 提到 Task Scheduler，眾所皆知的選項就是 TaskScheduler 以及 TaskDefinition 兩個類別。不過對於這個專案，有點殺雞用牛刀的感覺，所以此處用了 Timer 簡單勾勒一下。

:notebook: Timer Class
:notebook: DateTime Class
### Start/Stop Windows System Service
This feature is simply an example about how to use C# to manage Windows System Service

簡單的小範例，實驗如何用 C# 控制 Windows System Service。

```
Wolframcarbid.exe -wc=ctrl -act={start | stop} -svc={name}
```

:notebook: ServiceController Class
### Set SQL database location and user/password
A simple command to set SQL server location and logon user/password. The password is encrypted in configuration XML file.

提供命令能設定 SQL 伺服器位置及登入帳密，密碼以加密形式儲放於 XML 檔。
```
Wolframcarbid.exe -wc=dbm -src={location} -user={account} -pwd={password}
```
:notebook: XmlDocument Class
:notebook: XmlNode Class
:notebook: XmlElement Class
### New Tapei City Bus Arrival Time Query

This feature is simply an example about how to use C# to create a HTTP Client and how to manage JSON data.

查詢新北市公車到站時間，這也是個實驗，練習如何用 C# 開啟 HTTP Client 以及如何解析 JSON 資料。

```
Wolframcarbid.exe -wc=bus -stop={stop_name} -bound={in | out}
```

:notebook: HttpClient Class
:notebook: HttpResponseMessage Class
:notebook: JObject Class

### Taiwan Air Quality Index and PM2.5 Query

This feature is an example to demonstrate how to use C# to convert Taiwan Air Quality JSON data and insert in Microsoft SQL server. The data source is Taiwan Environment Protection Administration.

查詢台灣環保署空氣品質資料，包含 AQI，PM 2.5 及其他汙染物。同時也練習如何用 C# 將 JSON 資料儲存於 Microsoft SQL 伺服器。

```
Wolframcarbid.exe -wc=pm25 -loc={location_name}
If location_name = "xml", it will search Wolframcarbid.xml for a list with multiple locations
```

:notebook: WebClient Class
:notebook: JObject Class
:notebook: SqlConnection Class
:notebook: SqlCommand Class
:notebook: SqlDataReader Class

Richard, Chih-Yao Sun

Taipei (台北)


