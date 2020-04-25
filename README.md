# Настройка окружения Linux/macOS

Для понимания, в этой задаче 1% программирования и 99% администрирования. Так что, дальнейшая инструкция в основном для Devops и причастных.

Со стороны кода нужно подключить пакет [Microsoft.AspNetCore.Authentication.Negotiate](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.Negotiate) и настроить аутентификацию:

```Startup.cs
services
   .AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();
```

```Startup.cs
app.UseAuthentication();
app.UseMvc();
```

* Тестовое приложение: https://github.com/temaweb/negotiate (Core 3.1)
* Запуск приложения: `dotnet run --urls="http://hostname:port"`

Моя сеть:

* 192.168.0.23 — Клиент, Windows 10
* 192.168.0.66 — Клиент/Сервер приложения, macOS Catalina
* 192.168.0.82 — Контроллер домена, DNS, Windows Server 2012

## На DNS:

Создал A-запись

* Hostname: `application`
* FQDN: `application.temaweb.local`
* IP: `192.168.0.66`

## На контроллере домена:

(_PowerShell_)

1. Создал домен: `Install-ADDSForest –DomainName temaweb.local` 
2. Создал тестовых пользователей:
    ```powershell
    New-ADUser
        -GivenName Bob 
        -Surname Johnson 
        -Name "Bob Johnson" 
        -SamAccountName bob.johnson 
        -Enabled $True 
        -AccountPassword (ConvertTo-SecureString "Pass@word1!" -AsPlainText -force) 
        -PasswordNeverExpires $True
    
    New-ADUser 
        -GivenName Mary 
        -Surname Smith 
        -Name "Mary Smith" 
        -SamAccountName mary.smith 
        -Enabled $True 
        -AccountPassword (ConvertTo-SecureString "Pass@word1!" -AsPlainText -force) 
        -PasswordNeverExpires $True    
    ``` 
3. Добавил пользователя `bob.johnson` в администраторы: 
    ```powershell
    Add-ADGroupMember 
        -Identity "Domain Admins" 
        -Members "bob.johnson"
    ```
    
4. Создал SPN-записи
    ```cmd
    setspn -S HTTP/application.temaweb.local TEMAWEB\bob.johnson
    setspn -S HTTP/application@TEMAWEB.LOCAL TEMAWEB\bob.johnson
    ```
5. Сгенерировал Keytab и передал его на сервер приложения в ~/Downloads:
    ```cmd
    ktpass 
        -princ HTTP/application.temaweb.local@TEMAWEB.LOCAL 
        -pass Pass@word!1 
        -mapuser TEMAWEB\bob.johnson 
        -pType KRB5_NT_PRINCIPAL 
        -out krb.keytab
        -crypto AES256-SHA1
    ```

## На севрере приложения

1. Создал /etc/krb5.conf:
    ```
    [libdefaults]
    default_realm = TEMAWEB.LOCAL
    
    [realms]
    TEMAWEB.LOCAL = {
    	kdc = 192.168.0.82
    	default_domain = temaweb.local
    	admin_server = 192.168.0.82
    }
    
    [logging]
    kdc = FILE:/var/log/krb5/krb5kdc.log
    admin_server = FILE:/var/log/krb5/kadmind.log
    default = SYSLOG:NOTICE:DAEMON
            
    [domain_realm]
    .temaweb.local = TEMAWEB.LOCAL
    temaweb.local = TEMAWEB.LOCAL
    ```
2. Подключил keytab-файл и включил трейс:
    ```shell
        export KRB5_KTNAME=/Users/temaweb/Downloads/krb.keytab
        export KRB5_TRACE=/dev/stdout
    ```

3. Залогинился под _bob.johnson_: `kinit bob.johnson@TEMAWEB.LOCAL`
4. Залогинился под сервисом: `kinit HTTP/application.temaweb.local`
5. Результат команды `klist`:
    ```
    Credentials cache: API:A0A85444-541F-4F07-AA01-6CB778628DE7
            Principal: http/application.temaweb.local@temaweb.local
            
      Issued                Expires               Principal
    Apr 25 13:04:22 2020  Apr 25 23:03:21 2020  krbtgt/temaweb.local@temaweb.local
    Apr 25 13:05:10 2020  Apr 25 23:03:21 2020  http/application.temaweb.local@temaweb.local
    ```
6. Запускаем приложение из директории приложения: `dotnet run --urls="http://application.temaweb.local:8080`
7. Локально заходим на URL и проверяет:

![alt text](images/server-client.png?raw=true)

8. Сервер не должен содержать ошибок:

![alt text](images/server-log.png?raw=true)

## На клиенте

1. Входим в домен TEMAWEB.LOCAL как пользователь `mary.smith` и открываем `http://application.temaweb.local:8080/info`
2. Если пользователь залогинен и все наcтроено правильно то браузер должен передавать токен в ответ на `WWW-Authentication: Negotiate`
  ```
  [{"name":"mary.smith@TEMAWEB.LOCAL", "authType": "Kerberos"}]
  ```

















