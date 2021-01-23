Import-Module PowershellForXti -Force

$script:tempLogConfig = [PSCustomObject]@{
    RepoOwner = "JasonBenfield"
    RepoName = "TempLogServiceApp"
    AppName = "TempLog"
    AppType = "Service"
    ProjectDir = "Apps\TempLogServiceApp"
}

function TempLog-New-XtiIssue {
    param(
        [Parameter(Mandatory, Position=0)]
        [string] $IssueTitle,
        $Labels = @(),
        [string] $Body = "",
        [switch] $Start
    )
    $script:tempLogConfig | New-XtiIssue @PsBoundParameters
}

function TempLog-Xti-StartIssue {
    param(
        [Parameter(Position=0)]
        [long]$IssueNumber = 0,
        $IssueBranchTitle = "",
        $AssignTo = ""
    )
    $script:tempLogConfig | Xti-StartIssue @PsBoundParameters
}

function TempLog-New-XtiVersion {
    param(
        [Parameter(Position=0)]
        [ValidateSet("major", "minor", "patch")]
        $VersionType,
        [ValidateSet("Development", "Production", "Staging", "Test")]
        $EnvName = "Production"
    )
    $script:tempLogConfig | New-XtiVersion @PsBoundParameters
}

function TempLog-Xti-Merge {
    param(
        [Parameter(Position=0)]
        [string] $CommitMessage
    )
    $script:tempLogConfig | Xti-Merge @PsBoundParameters
}

function TempLog-New-XtiPullRequest {
    param(
        [Parameter(Position=0)]
        [string] $CommitMessage
    )
    $script:tempLogConfig | New-XtiPullRequest @PsBoundParameters
}

function TempLog-Xti-PostMerge {
    param(
    )
    $script:tempLogConfig | Xti-PostMerge @PsBoundParameters
}

function TempLog-Publish {
    param(
        [ValidateSet("Production", “Development", "Staging", "Test")]
        [string] $EnvName="Development"
    )
    $ErrorActionPreference = "Stop"

    Write-Output "Publishing to $EnvName"
    
    Write-Output "Building solution"
    dotnet build 
    
    Write-Output "Setting Up Temp Log"
    TempLog-Setup -EnvName $EnvName
    
    if($EnvName -eq "Production") {
        $branch = Get-CurrentBranchname
        Write-Output "Begin Publish"
        Xti-BeginPublish -BranchName $branch
    }
    Write-Output  "Publishing Temp Log"
    $script:tempLogConfig | Xti-PublishServiceApp @PsBoundParameters
    if($EnvName -eq "Production") {
        Write-Output "End Publish"
        Xti-EndPublish -BranchName $branch
        Write-Output "Merging Pull Request"
        $script:tempLogConfig | Xti-Merge
    }
}

function TempLog-Setup {
    param(
        [ValidateSet("Production", "Development", "Staging", "Test")]
        [string] $EnvName="Development"
    )
    dotnet build Apps/TempLogSetupApp
    if( $LASTEXITCODE -ne 0 ) {
        Throw "Temp Log setup failed with exit code $LASTEXITCODE"
    }
    dotnet run --project Apps/TempLogSetupApp --environment=$EnvName
    if( $LASTEXITCODE -ne 0 ) {
        Throw "Temp Log setup failed with exit code $LASTEXITCODE"
    }
}