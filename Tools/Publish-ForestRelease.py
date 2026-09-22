"""Run the established publisher with the project's existing GitHub executable."""
import importlib.util,pathlib,subprocess
root=pathlib.Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('publisher',root/'Tools/Publish-LauncherRelease.py')
publisher=importlib.util.module_from_spec(spec);spec.loader.exec_module(publisher)
publisher.gh=lambda *args: subprocess.run([str(root/'Builds/PublisherTools/gh-2.101.0/bin/gh.exe'),*args],check=True,capture_output=True,text=True).stdout
publisher.main()
