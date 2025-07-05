# GodotCSharp\_AnonymousFirestore

This library is meant to be used for a quick and easy simple anonymous data collection with Firebase in Godot.
The library supports anonymous authentication, that can be then used to create documents in Firestore.
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
		new FirestoreDocument(new Dictionary<string, object>() {
			{ "String value", new StringFirestoreValue("Bear") },
			{ "Boolean value", new BooleanFirestoreValue(true) },
			{ "Double value", new DoubleFirestoreValue(2.5) },
			{ "Array", new ArrayFirestoreValue(
				new ArrayValuesFirestoreValue(new List<object>()
				{
					new StringFirestoreValue("Value 1"),
					new DoubleFirestoreValue(2)
				}))},
			{ "Map", new MapFirestoreValue(
				new MapFieldsFirestoreValue(new Dictionary<string, object>() {
					{ "Random", new StringFirestoreValue("Bear") },
					{ "Bool", new BooleanFirestoreValue(true) },
					{ "Double", new DoubleFirestoreValue(2.5) }
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

#### Why didn't you implement X from Firebase?

This is meant to be used as a way to upload anonymous data from players gameplay. It's not meant to be used to maintain the player profile data (e.g. what they unlocked in the game or what are their stats), as that is something that you should be using OAuth methods for and be able to update the documents, which neither is provided in this implementation.

This library is meant to be used to send one-of queries to create data about e.g. most recent run the player did in the game and what they collected in that run for further data analysis.

Since you are providing the data in FirestoreDocument, it is your responsibility to ensure that the data stored DOES NOT contain PII (Personally Identifiable Information) in it.

#### What are the future plans?

There should be a better way of doing the JSON formatted classes for easier data manipulation than what I have here. There is also no error handling right now (it will just throw exception) - I might add a wrapper around it and include events to listen to in case a failure occurs.

#### This is awesome! How can I support you?

This is just a small little library - if you found it helpful, then that already made my day!

I'm a solo indie game developer, so if you'd like to [checkout my games](https://store.steampowered.com/search/?developer=Rainier%20Interactive) that would be amazing!

