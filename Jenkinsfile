pipeline {
    agent none
    stages {
        stage('Build') {
            agent { label 'windows' }
            steps {
                bat "dotnet build"
            }
        }
        stage('Unit tests') {
            agent { label 'windows' }
            options { skipDefaultCheckout() }
            environment {
                GIT_VERSION = """${bat(
                    returnStdout: true,
                    script: '@git describe --tags --always'
                ).trim()}"""
            }
            steps {
                bat "dotnet test --no-build --logger:\"junit;LogFilePath=quickwarden-${GIT_VERSION}-unit-tests.xml\" --collect:\"XPlat Code Coverage\" --results-directory artifacts\\reports"
                bat "move /y src\\Quickwarden.Tests\\quickwarden-${GIT_VERSION}-unit-tests-*.xml artifacts\\reports\\"
                dir('artifacts/reports') {
                    bat "tar -a -c -f quickwarden-${GIT_VERSION}-unit-tests.zip quickwarden-${GIT_VERSION}-unit-tests-*.xml"
                }
                junit "artifacts/reports/quickwarden-${GIT_VERSION}-unit-tests-*.xml"
                recordCoverage(tools: [[parser: 'COBERTURA', pattern: 'artifacts/reports/*/coverage.cobertura.xml']], sourceCodeRetention: 'EVERY_BUILD')
                archiveArtifacts artifacts: "artifacts/reports/quickwarden-${GIT_VERSION}-unit-tests.zip"
            }
        }
        stage('Package (Windows)') {
            agent { label 'windows' }
            options { skipDefaultCheckout() }
            environment {
                GIT_VERSION = """${bat(
                    returnStdout: true,
                    script: '@git describe --tags --always'
                ).trim()}"""
            }
            steps {
                dir('scripts') {
                    bat "powershell .\\build-windows.ps1"
                }
                archiveArtifacts artifacts: "dist/quickwarden-${GIT_VERSION}-windows*.zip, dist/quickwarden-${GIT_VERSION}-windows*.exe"
            }
        }
    }
}