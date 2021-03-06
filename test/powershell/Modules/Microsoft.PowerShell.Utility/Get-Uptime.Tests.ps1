Describe "Get-Uptime" -Tags "CI" {
    BeforeAll {
        $IsHighResolution = [system.diagnostics.stopwatch]::IsHighResolution
        # Skip Get-Uptime test if IsHighResolution = false
        # because stopwatch.GetTimestamp() return DateTime.UtcNow.Ticks
        # instead of ticks from system startup
        if ( ! $IsHighResolution )
        {
            $origDefaults = $PSDefaultParameterValues.Clone()
            $PSDefaultParameterValues['it:skip'] = $true
        }
    }
    AfterAll {
        if ( ! $IsHighResolution ){
            $global:PSDefaultParameterValues = $origDefaults
        }
    }
    It "Get-Uptime return timespan (default -Timespan)" {
        $upt = Get-Uptime
        $upt | Should Not Be $null
        ($upt).Gettype().Name | Should Be "Timespan"
    }
    It "Get-Uptime -Since return DateTime" {
        $upt = Get-Uptime -Since
        $upt | Should Not Be $null
        ($upt).Gettype().Name | Should Be "DateTime"
    }
    It "Get-Uptime throw if IsHighResolution == false" {
        try
        {
            # Enable the test hook
            [system.management.automation.internal.internaltesthooks]::SetTestHook('StopwatchIsNotHighResolution', $true)

            Get-Uptime
            throw "No Exception!"
        }
        catch
        {
            $_.FullyQualifiedErrorId | Should be "GetUptimePlatformIsNotSupported,Microsoft.PowerShell.Commands.GetUptimeCommand"
        }
        finally
        {
            # Disable the test hook
            [system.management.automation.internal.internaltesthooks]::SetTestHook('StopwatchIsHighResolutionIsFalse', $false)
        }
    }
}
