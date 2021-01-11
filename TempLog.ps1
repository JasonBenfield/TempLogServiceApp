Import-Module PowershellForXti -Force

$script:tempLogConfig = [PSCustomObject]@{
    RepoOwner = "JasonBenfield"
    RepoName = "TempLogServiceApp"
    AppName = "TempLog"
    AppType = "Service"
    ProjectDir = "C:\XTI\src\TempLogServiceApp\Apps\TempLogServiceApp"
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

    $activity = "Publishing to $EnvName"
    
    Write-Progress -Activity $activity -Status "Building solution" -PercentComplete 50
    dotnet build 
    
    Write-Progress -Activity $activity -Status "Publishing service app" -PercentComplete 80
    if($EnvName -eq "Production") {
        $branch = Get-CurrentBranchname
        Xti-BeginPublish -BranchName $branch
    }
    $script:tempLogConfig | Xti-PublishServiceApp @PsBoundParameters
    if($EnvName -eq "Production") {
        Xti-EndPublish -BranchName $branch
        $script:tempLogConfig | Xti-Merge
    }
}
