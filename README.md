# MigrateYamlPipelineTools
Migrate Yaml Pipeline Tools

```shell
MigrateYamlPipeline.exe -y "C:\pipeline\YamlPipeline.yml" -c "C:\pipeline\ClassicPipeline.json"
```

VSTest Migrate:

```shell
MigrateYamlPipeline.exe -y "C:\pipeline\YamlPipeline.yml" -c "C:\pipeline\ClassicPipeline.json" --vstest --testpool CustomPool --custompool xxx
```

Update Environment Permissions:

```shell
MigrateYamlPipeline.exe -y "C:\pipeline\YamlPipeline.yml" -o None --envpermissions --org xxx --project xxx --pat xxx --copyenv xxx
```
> Batch update the Environment Permissions of all yml files in a folder: `MigrateYamlPipeline.exe -y "C:\pipeline" -o None --envpermissions --org xxx --project xxx --pat xxx --copyenv xxx`

Add Pre Approval:

```shell
MigrateYamlPipeline.exe -y "C:\pipeline\YamlPipeline.yml" -c "C:\pipeline\ClassicPipeline.json" -o None --addpreapproval --org xxx --project xxx --pat xxx
```

> Note: Personal Access Token (PAT) Scopes
> - Build - Read 
> - Environment - Read & manage 
> - Identity - Read
> - Pipeline Resources - Use and manage 
> - Security - Manage

Additional command line arguments:

1. `-y, --yaml` Required. Yaml Pipeline File Path
1. `-c, --classic` Required. Classic Pipeline File Path
1. `-o, --output` OutPut Yaml File Path [None/FilePath]
1. `--org` Organization
1. `--project` Project
1. `--pat` Personal Access Token (PAT)
1. `--addpreapproval` (Group: Add Pre Approval Option) (Default: false) Is Add Pre Approval
1. `--envpermissions` (Group: Update Environment Permissions Option) (Default: false) Is Update Environment Permissions
1. `--copyenv`  (Group: Update Environment Permissions Option) Copy Permissions Environment
1. `--vstest` (Group: VSTest Migrate Option) (Default: false) Is Migrate VSTest
1. `--testpool` (Group: VSTest Migrate Option) (Default: CloudTest) Test Pool Type [CloudTest/CustomPool]
1. `--testpipeline` (Group: VSTest Migrate Option) (Default: cdnrp-Buddy) Test Pipeline Name
1. `--testartifact` (Group: VSTest Migrate Option) (Default: drop_build_test) Test Artifact Name
1. `--custompool` (Group: VSTest Migrate Option) (Default: cdnrp-agentpool) Custom Pool
1. `--cloudtest-connected` (Group: VSTest Migrate Option) (Default: CloudTest-Prod) Cloud Test Service Name
1. `--cloudtest-tenant` (Group: VSTest Migrate Option) (Default: cdncloudtesttenant) Cloud Test Tenant
1. `--cloudtest-alias` (Group: VSTest Migrate Option) (Default: cdnrp) Cloud Test Schedule Build Requester Alias
1. `--cloudtest-maplocation` (Group: VSTest Migrate Option) (Default: [BuildRoot]\CloudTests\TestMap.E2E.xml) Cloud Test Test Map Location
