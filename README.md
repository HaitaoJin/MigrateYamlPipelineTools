# ChinaCDNToolsPrivate
China CDN Tools Private

## MigrateYamlPipeline

```shell
MigrateYamlPipeline.exe -y "C:\pipeline\YamlPipeline.yml" -c "C:\pipeline\ClassicPipeline.json"
```

Additional command line arguments:

 1.  `-y, --yaml`                 Required. Yaml Pipeline File Path
 1.  `-c, --classic`              Required. Classic Pipeline File Path
 1.  `-o, --output`               OutPut Yaml File Path
 2.  `--testpool`                 (Group: VSTest Migrate Option) (Default: CloudTest) Test Pool Type [CloudTest/CustomPool]
 3.  `--testpipeline`             (Group: VSTest Migrate Option) (Default: cdnrp-Buddy) Test Pipeline Name
 4.  `--testartifact`             (Group: VSTest Migrate Option) (Default: drop_build_test) Test Artifact Name
 5.  `--custompool`               (Group: VSTest Migrate Option) (Default: cdnrp-agentpool) Custom Pool
 6.  `--cloudtest-connected`      (Group: VSTest Migrate Option) (Default: CloudTest-Prod) Cloud Test Service Name
 8.  `--cloudtest-tenant`         (Group: VSTest Migrate Option) (Default: cdncloudtesttenant) Cloud Test Tenant
 9.  `--cloudtest-alias`          (Group: VSTest Migrate Option) (Default: cdnrp) Cloud Test Schedule Build Requester Alias
 10. `--cloudtest-maplocation`    (Group: VSTest Migrate Option) (Default: [BuildRoot]\CloudTests\TestMap.E2E.xml) Cloud Test Test Map Location