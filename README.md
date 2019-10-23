# evocontest
.NET coding contest website, runner and SDK for evosoft Hungary Miskolc, with a heavy focus on performance. 
The contest website is available at: [https://evocontest.azurewebsites.net](https://evocontest.azurewebsites.net)

# Setting up the WebApp locally
*evocontest.WebApp* and *evocontest.Runner.Host* both have to be running to allow all features of the website to work. Follow these steps if you want to set up the WebApp locally:

1. An administrator and a worker account have to be defined for the web application. The worker user will be used by the *evocontest.Runner.Host* application. An API key for SendGrid is needed to send confirmation emails after registration. Fill out the AdminEmail, WorkerEmail and SendGridKey user secrets for *evocontest.WebApp*.
2. Create a local database by executing the *Create-Database* command in the Package Manager Console.
3. Start the WebApp, and register the administrator and worker accounts.
4. Provide the login details for the worker account in *evocontest.Runner.Host* appsettings.json or user secrets.
5. Execute *evocontest.Runner.Host*. Now solution submitting and validation should be working. A match can be started on the Admin page of the web application.

# Project structure
- **evocontest.Common**
  The only allowed dependency of submissions.
- **evocontest.Runner.Common**
  Shared code required for generating inputs and running solutions.
- **evocontest.Runner.Host**
  Console application for running solution, and creating daily rankings.
- **evocontest.Runner.Host.Common**
  Shared code between Runner.Host and Runner.Worker, mainly responsible for communication.
- **evocontest.Runner.RaspberryPiUtilities**
  Handling CPU fan and E-paper display on the Raspberry Pi.
- **evocontest.Runner.SubmissionEnvironment**
  Empty project responsible for referencing all dependencies a submission needs to be executed. Used to set up the execution environment in a sandbox for a submission.
- **evocontest.Runner.Worker**
  Console application intended to be running in a sandbox. Executes the commands received from the Host application, including Loading, Testing and running a user submitted submission.
- **evocontest.Submission**
  Template project for a user submission, containing an empty solution.
- **evocontest.Submission.Runner**
  Simplified console application, responsible for testing out the performance of a user submission by the user.
- **evocontest.Submission.Sample**
  Sample implementation for a submission.
- **evocontest.Submission.Test**
  Unit tests for the sample implementation and user submission.
- **evocontest.WebApp**
  The website for the contest.
- **evocontest.WebApp.Common**
  Shared code between the WebApp and Host application. Contains constants, relative addresses, API definitions and DTOs.
- **evocontest.WebApp.Placeholder**
  A simple website containing basic information of the contest. It was used as a placeholder, before the main site was activated.
- **Prototypes**
  Small projects to test out contest related logic, such as manipulating the raspberry pi peripherals, breaking out of the sandbox, efficiency of the input generation, and the organizer's own submission for the contest.





