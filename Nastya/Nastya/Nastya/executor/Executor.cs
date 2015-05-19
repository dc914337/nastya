﻿using Nastya.Nastya.config;
using Nastya.Nastya.executor.commands;
using Nastya.Nastya.executor.context;
using Nastya.Nastya.logger;
using Nastya.Nastya.messenger;

namespace Nastya.Nastya.executor
{
    class Executor
    {
        private NastyaCommand[] nastyaCommands;

        public Executor(Config config)
        {
            //master commit
            nastyaCommands = config.Commands;
        }

        public void ProcessMessage(Message message)
        {
            Logger.Out("Processing MessageBody \"{0}\" from {1}({2})", MessageType.Verbose, message.MessageBody, message.From, message.Source);
            ExecuteCommands(message);
        }

        private void ExecuteCommands(Message message)
        {
            NastyaCommand priorCommand = null;
            int maxRate = -1;
            foreach (var nastyaCommand in nastyaCommands)
            {
                var checkResult = nastyaCommand.CheckCommandFits(message);
                if (checkResult == null || checkResult.Type != CheckResults.Success) continue;

                if (priorCommand == null ||
                    priorCommand.Priority < nastyaCommand.Priority ||
                    (priorCommand.Priority == nastyaCommand.Priority && (int)checkResult.Rate > maxRate))
                {
                    priorCommand = nastyaCommand;
                    maxRate = checkResult.Rate;
                }
            }

            if (priorCommand == null)
            {
                Logger.Out("Didn't find any command.. {0}", MessageType.Debug, message.MessageBody);
            }
            else
            {
                Logger.Out("Found command: {0}. Source MessageBody: {1}", MessageType.Debug, priorCommand.CommandId, message.MessageBody);
                priorCommand.Execute(message);
            }

        }
    }
}

