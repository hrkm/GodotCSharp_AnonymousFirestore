# GodotCSharp\_AnonymousFirestore

This library is meant to be used for a quick and easy simple anonymous data collection with Firebase in Godot.
The library supports anonymous authentication, that can be then used to create documents in Firestore.
The library also has a simple Google Analytics implementation for sending basic event data.
It is using the REST APIs of the Firebase.Authentication and Cloud.Firestore functions.
It is also using the Godot FileAccess/DirAccess classes to store the authentication data in a local encrypted file.

#### Setup
Download the repository and copy into your Godot project.

Somewhere in your code initialize the Firebase logic like this:
```
var auth = new Firebase.Auth(<your_api_key>);
auth.LoggedIn += (s, e) =>
{
	var firestore = new Firebase.Firestore(<your_project_id>, e.IdToken);
	firestore.CreateDocument(<your_collection_id>, e.UserId,
		new FirestoreDocument(new Dictionary<string, FirestoreValue>() {
			{ "String value", new FirestoreValue("Bear") },
			{ "Boolean value", new FirestoreValue(true) },
			{ "Double value", new FirestoreValue(2.5) },
			{ "Array", new FirestoreValue(
				new ArrayValuesFirestoreValue(new List<FirestoreValue>()
				{
					new FirestoreValue("Value 1"),
					new FirestoreValue(2)
				}))},
			{ "Map", new FirestoreValue(
				new MapFieldsFirestoreValue(new Dictionary<string, FirestoreValue>() {
					{ "Random", new FirestoreValue("Bear") },
					{ "Bool", new FirestoreValue(true) },
					{ "Double", new FirestoreValue(2.5) }
				}))},
	}));
};
auth.LoginAnonymously();

```

Using Firebase console, change the Cloud Firestore rules by adding the following entry:
```
match /<your_collection_id>/{uid} {
     allow read, write: if request.auth != null && request.auth.uid == uid;
   }
```

This will only allow a given authenticated user the ability to write a document with the title matching their user id. That way you are guaranteed to not access other data in the Firestore, and if anyone decides to spam your database they can only access the data they create with those anonymous accounts.

Please note that you can set a quota of how many users can be created from a given IP address in Firebase.Authentication Settings in the Firebase console.

#### Using Google Analytics
Download the repository and copy into your Godot project.

Somewhere in your code initialize Google Analytics logic like this:
```
var analytics = new Firebase.Analytics(<analytics_secret_api_key>, <measurement_id>, <app_instance_id>);
analytics.UserId = <user_id>; // optional, allows to differentiate the users and provide counts of active users
analytics.SendScreenViewEvent("MyScene");
analytics.SendEvent(new Analytics.Event("my custom event", new Dictionary<string, object>() { { "param1", 23 } }));
```

#### Where do I get the different keys and values for the configuration?

<your_api_key> - API key from the Firebase, open Project Settings and find your app in the list, there should be a configuration section with your apiKey and projectId

<your_project_id> - the id of the project as it appears in https://console.firebase.google.com/project/{project-id} (or in the config mentioned above)

<your_collection_id> - any name for the collection you want, just a string

<measurement_id> - this is from Google Analytics, go to Admin > Data Streams -> (select your stream) -> the id should be in the upper right corner, starts with "G-"

<analytics_secret_api_key> - this is also in Data Streams section, after selecting a stream you have to open "Measurement Protocol API secrets" and generate a new secret

<app_instance_id> - it's supposed to be 32 character long hexadecimal string, "00000000000000000000000000000000" works just fine? :)

<user_id> - if you are using the Firebase.Authentication, you can use that to disambiguate the Google Analytics users



#### Why didn't you implement X from Firebase?

This is meant to be used as a way to upload anonymous data from players gameplay. It's not meant to be used to maintain the player profile data (e.g. what they unlocked in the game or what are their stats), as that is something that you should be using OAuth methods for and be able to update the documents, which neither is provided in this implementation.

This library is meant to be used to send one-of queries to create data about e.g. most recent run the player did in the game and what they collected in that run for further data analysis.

Since you are providing the data in FirestoreDocument, it is your responsibility to ensure that the data stored DOES NOT contain PII (Personally Identifiable Information) in it.

#### What are the future plans?

There is also no error handling right now (it will just throw exception) - I might add a wrapper around it and include events to listen to in case a failure occurs. I may consider adding more of the standard Analytics events depending on the results.

#### This is awesome! How can I support you?

This is just a small little library - if you found it helpful, then that already made my day!

I'm a solo indie game developer, so if you'd like to [checkout my games](https://store.steampowered.com/search/?developer=Rainier%20Interactive) that would be amazing!

