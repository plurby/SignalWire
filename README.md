# SignalWire #

/* Make sure the connection string in the demo project is proper and your SQL server instance is running while you run the demo */

SignalWire is an experimental Server<->Client plumbing project, that magically wires up your HTML5 front end and your data store. SignalWire uses SignalR and Roslyn libraries to implement features like

* Exposing collections in your Back end directly over the wire
* Use C# in your HTML applications.

As of now, support is available for EntityFramework and MongoDb as the back end. Also, it provides

* A light weight permission framework (wip)
* A Client side Javascript API to access your collections with minimal/no serverside code
* Model Validation support using normal C#/ASP.NET Data Validation attributes
* LINQ support to issue Linq queires from HTML pages (highly experimental, as of now no sandboxing support per caller)
* C# code in your HTML pages (wip)

## How To Start ##

You may start with Creating an Empty ASP.NET Project in Visual Studio 2012 (You need .NET 4.5 as we are using Roslyn September CTP libraries). Then, install SignalWire using Nuget. In Package Manager console

> Install-Package SignalWire

This will add the required SignalWire dependencies and references. Also, the following files will be added to your project, as a starting point/example.

* Models\TaskDb.cs - An example Entity Framework Data Context. You can use your instead 
* Hubs\DataHub.cs - An example Datahub.
* Scripts\SignalWire.js - Clientside JQuery Pluin that Wraps SignalR hubs to provide $.wire.

Now, you may goto Index.html, and ensure all your JS file versions are correct, and run the project with index.html as the startup page.

## Server side - The Data hub and Poco Classes ##
The Only server side code you need is your POCO objects/ORM and a hub inherited from the DataHub base class, defined in the SignalWire library. You need to use the Collections attribute to map a POCO class with the set/collection, and make sure you always do that in lower case.

You may also use the Validation related annotation attributes as these attributes are supported for Model validation.

```
   //Simple POCO class to represent a task
    [Collection("tasks")]
    public class Task
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Subject cannot be longer than 40 characters.")]
        public string Subject { get; set; }

        [Required]
        [MaxLength(200, ErrorMessage = "Details cannot be longer than 40 characters.")]
        public string Details { get; set; }

        public bool Completed { get; set; }
    }
 
 
   //Simple demo db context   
    public class TaskDb : DbContext
    {
        public DbSet<Task> Tasks { get; set; }
    }
```

Now, you need a hub that uses TaskDb DbContext. By convention, Wire expects the server hub�s name as 'Data' if it is not specified in the init method of $.wire.init(..). 

In the example you get (when you install the Nuget package), you�ll see we are using an Entity Framework Context Provider. Replace TaskDb with your own EF Data context if required. Once you do that, all your collections/sets with in the context can be accessed over the wire. 

```
public class Data : DataHub<EFContextProvider<TaskDb>> 
```

This will expose the collections in TaskDb over the wire, and that's all you need to access collections as shown below. 

## Client Side - Initializing and Issuing Queries ##

SignalWire magically exposes all the Tables/Sets/Collections in your Data store back end via the $.wire Javascript object at client side. You can initialize $.wire using the init() method, which returns a JQuery Deferred. Here is a quick example regarding initializing Wire and issuing a LINQ query.

```
     
	 //Initialize wire
	 $.wire.init().done(function () {

      //Now you can access the tasks collection in your data context  
	  //You can issue a LINQ Query.
      $.wire.tasks.query("from Task t in Tasks select t")
	           .done(function (result) {
                    $("#task-list").html(result.length + " records found<hr/>");
                    $.each(result, function (index, task) {
                        //do something with the task
                    });
                }).fail(function (result) {
                    alert(JSON.stringify(result.Error));
                });
		});		
```

##Other Wire Methods##

You can use $.wire.yourcollection.add(..) to add objects to a collection. The add method will return the added item with updated Id up on completion.

```
	var t = {
		"subject": $("#subject").val(),
		"details": $("#details").val(),
	};


	//Add a task to the Tasks collection                    
	$.wire.tasks.add(t).done(function (task) {
		//Note that you'll get the auto generated Id
		//task.Id will be valid
		
	}).fail(function (result) {
			//result.Error contains the error if any                        
			//result.ValidationResults contains the Validation results if any 
			alert(JSON.stringify(result.Error));
	});
```				

Similarly, you can use 

* $.wire.yourcollection.remove(item) to remove items
* $.wire.yourcollection.update(item) to update an item (based on Id match).
* $.wire.yourcollection.read({"query":"expression", "skip":"xxx", "take":"xxx"}) to read from a specific collection

All the above methods will return a JQuery Deferred object, so you can use .done(..) and .fail(..) as in the above add example.

##Permissions##

You can decorate your Model classes with Custom permission attributes that implements IPermission. This will check if the user in the current context can actually perform a specific operation on a Model entity.


##What's More##

I'm planning the following features based on my time

* Sandboxed execution for C# scripts in HTML page that can access client side context
* Publishing and data sync
* Permission based events, may be using something like PushQA.

Read more here in my blog
* [Introducing SignalWire] (http://www.amazedsaint.com/2012/09/signalwire-magical-plumbing-with-your.html)

