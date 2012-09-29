# SignalWire #

SignalWire is an experimental Server<->Client plumbing project, that magically wires up your HTML5 front end and to your data store. SignalWire uses SignalR and Roslyn libraries to implement features like

*Exposing collections in your Back end directly over the wire
*Enable using C# in your HTML applications.

As of now, support is available for EntityFramework and MongoDb as the back end. Also, it provides

*A light weight permission framework (wip)
*A Client side Javascript API to access your collections with minimal/no serverside code
*Model Validation support using normal C#/ASP.NET Data Validation attributes
*LINQ support to issue Linq queires from HTML pages (highly experimental, as of now no sandboxing support per caller)
*C# code in your HTML pages (wip)

## How To Start ##

You may start with Creating an Empty ASP.NET Project in Visual Studio 2012 (You need .NET 4.5 as we are using Roslyn September CTP libraries). Then, install SignalWire using Nuget. In Package Manager console

> Install-Package SignalWire

This will add the following components to your project.

* Models\TaskDb.cs - An example Entity Framework Data Context. You can use your instead 
* Hubs\DataHub.cs - An example Datahub.
* Scripts\SignalWire.js - Clientside JQuery Pluin for SignalR.

Now, goto Index.html, and verify all your JS file versions are correct.

## Initializing and Issuing Queries ##

SignalWire magically exposes all your Tables/Sets/Collections in your Data back end via the $.wire Javascript object at client side. You can initialize $.wire using the init() method, which returns a JQuery Deferred. Here is a quick example regarding initializing Wire and issuing a LINQ query.

```
     
	 //Initialize wire
	 $.wire.init().done(function () {

      //Now you can access the tasks collection in your data context  
	  //You can issue a LINQ Query.
      $.wire.tasks.query("from Task t in Tasks select t")
	           .done(function (result) {
                    $("#task-list").html(result.length + " records found<hr/>");
                    $.each(result, function (index, task) {
                        addTask(task);
                    });
                }).fail(function (result) {
                    alert(JSON.stringify(result.Error));
                });
```

##Other Wire Methods##

You can use $.wire.yourcollection.add(..) to add objects. Like this. The add method will return the added item with updated Id up on completion.

```
	var t = {
		"subject": $("#subject").val(),
		"details": $("#details").val(),
	};


	//Add a task to the Tasks collection                    
	$.wire.tasks.add(t).done(function (task) {
		//Note that you'll get the auto generated Id
		addTask(task);                        
	}).fail(function (result) {
			//result.Error contains the error if any                        
			//result.ValidationResults contains the Validation results if any 
			alert(JSON.stringify(result.Error));
			alert(JSON.stringify(result.Error)); });
```				

Similarly, you can use 

* $.wire.yourcollection.remove(item) to remove items
* $.wire.yourcollection.update(item) to update an item (based on Id match).
* $.wire.yourcollection.read({"query":"expression", "skip":"xxx", "take":"xxx"}) to read from a specific collection

##Permissions##

You can decorate your Model classes with Custom permission attributes that implements IPermission. This will check if the user in the current context can actually perform a specific operation on a Model entity.


##What's More##

I'm planning the following features based on my time

*Sandboxed execution for C# scripts in HTML page that can access client side context
*Publishing and data sync
*Permission based events, may be using something like PushQA.


