# mobirise-angular
Build yout Angular-HTML-Templates easily and fast with the help of the mobirise webside builder.

# Prerequisite
- Windows Machine
- Mobirise desktop application (https://mobirise.com/)
- Node.js
- Angular

# Getting Started
1. Download this repository as ZIP and extract it to the location of your choice.
2. Run the `initialisation-script.cmd` file to initially setup your project.
3. After the previous script has finished, run `start-change-detection-script.cmd`. 
    1. This will start the FileWatcher which listens for file changes in your Mobirise-Template and applies them to your Angular project.
    2. Your Angular application is now running under localhost:4200
4. While the script is still running open the Mobirise application and start designing your webpage.    
    1. When you're done, publish the project to `<your-repository-location>/mobirise-template` **Attention :** its crucial to publish exactly to this location because otherwise the FileWatcher won't detect the changes
5. Navigate to `localhost:4200` and see your mobirise design apllied to your Angular application
