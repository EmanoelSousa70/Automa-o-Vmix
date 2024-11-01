' Número da entrada do GT Timer - Timer Center para exibir ambos os tempos
Dim timerInputNumber As Integer = 2 ' Ajuste conforme necessário

' Carrega o XML do vMix para obter a entrada ativa (PGM)
Dim xmlDoc As New Xml.XmlDocument()

' Loop contínuo
Do While True
    ' Atualiza o XML para verificar a entrada ativa no PGM
    xmlDoc.LoadXml(API.XML)

    ' Obtém o número da entrada ativa no PGM
    Dim activeInput As String = xmlDoc.SelectSingleNode("//vmix/active").InnerText

    ' Verifica se a entrada ativa possui duração e posição
    Dim inputNode = xmlDoc.SelectSingleNode("//vmix/inputs/input[@number='" & activeInput & "']")
    If inputNode IsNot Nothing Then
        Dim durationNode = inputNode.Attributes("duration")
        Dim positionNode = inputNode.Attributes("position")

        ' Depuração: Mostrar o número da entrada ativa
        Console.WriteLine("Entrada Ativa: " & activeInput)

        If durationNode IsNot Nothing AndAlso positionNode IsNot Nothing Then
            ' Converter duração total para milissegundos
            Dim totalMilliseconds As Integer = Integer.Parse(durationNode.Value)
            Dim positionMilliseconds As Integer = Integer.Parse(positionNode.Value)

            ' Checa se a diferença entre a duração total e a posição atual é de 1 segundo
            Dim detectedDurationSeconds As Integer = totalMilliseconds \ 1000
            Dim detectedPositionSeconds As Integer = positionMilliseconds \ 1000

            If detectedDurationSeconds - detectedPositionSeconds = 1 Then
                ' Adiciona 1 segundo apenas se o vídeo parecer estar "1 segundo curto"
                detectedDurationSeconds += 1
                totalMilliseconds = detectedDurationSeconds * 1000 ' Atualiza totalMilliseconds para refletir a correção
            End If

            ' Converter duração total ajustada para minutos e segundos
            Dim totalMinutes As Integer = totalMilliseconds \ 60000
            Dim totalSeconds As Integer = (totalMilliseconds Mod 60000) \ 1000
            Dim durationText As String = String.Format("{0:D2}:{1:D2}", totalMinutes, totalSeconds)

            ' Calcular o tempo restante em segundos
            Dim remainingTime As Integer = (totalMilliseconds - positionMilliseconds) \ 1000

            ' Atualizar o Timer enquanto o vídeo está ativo
            While remainingTime >= 0 AndAlso xmlDoc.SelectSingleNode("//vmix/active").InnerText = activeInput
                ' Calcular minutos e segundos para o tempo restante
                Dim minutes As Integer = remainingTime \ 60
                Dim seconds As Integer = remainingTime Mod 60

                ' Combinar tempo total e contagem regressiva em um único texto
               Dim timerText As String = String.Format("Total: {0}" & Environment.NewLine & "Restante: {1:D2}:{2:D2}", durationText, minutes, seconds)

                ' Definir o texto no GT Timer
                API.Function("SetText", Input:=timerInputNumber.ToString(), Value:=timerText)

                ' Log de depuração para verificar o texto do timer
                Console.WriteLine("Atualizando GT Timer para: " & timerText)

                ' Esperar 1 segundo antes de atualizar a posição
                Threading.Thread.Sleep(1000)

                ' Atualiza a posição atual do vídeo
                xmlDoc.LoadXml(API.XML)
                positionMilliseconds = Integer.Parse(xmlDoc.SelectSingleNode("//vmix/inputs/input[@number='" & activeInput & "']/@position").Value)
                remainingTime = (totalMilliseconds - positionMilliseconds) \ 1000
            End While

            ' Limpar o GT Timer quando o vídeo termina
            API.Function("SetText", Input:=timerInputNumber.ToString(), Value:="Total: " & durationText & "  |  Restante: 00:00")
        Else
            ' Log de depuração
            Console.WriteLine("Duração ou posição não encontradas para a entrada: " & activeInput)
        End If
    Else
        ' Log de depuração
        Console.WriteLine("Entrada não encontrada para: " & activeInput)
    End If

    ' Intervalo para verificar mudanças na entrada ativa (PGM)
    Threading.Thread.Sleep(500)
Loop
